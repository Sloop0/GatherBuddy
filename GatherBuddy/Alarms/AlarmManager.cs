﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Dalamud.Game;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Logging;
using GatherBuddy.Interfaces;
using GatherBuddy.Plugin;
using GatherBuddy.SeFunctions;
using GatherBuddy.Time;
using Newtonsoft.Json;

namespace GatherBuddy.Alarms;

public partial class AlarmManager : IDisposable
{
    private const    string    FileName = "alarms.json";
    private readonly PlaySound _sounds;

    public   List<AlarmGroup>                  Alarms        { get; init; } = new();
    internal Dictionary<Alarm, TimeStamp>      ActiveAlarms  { get; init; } = new();
    public   (Alarm, ILocation, TimeInterval)? LastItemAlarm { get; private set; }
    public   (Alarm, ILocation, TimeInterval)? LastFishAlarm { get; private set; }

    public TimeStamp NextChange = TimeStamp.Epoch;

    public AlarmManager()
        => _sounds = new PlaySound(Dalamud.SigScanner);

    public void Enable()
    {
        if (GatherBuddy.Config.AlarmsEnabled)
            return;

        GatherBuddy.Config.AlarmsEnabled = true;
        GatherBuddy.Config.Save();

        Dalamud.Framework.Update  += OnUpdate;
        Dalamud.ClientState.Login += OnLogin;
        SetActiveAlarms();
    }

    internal void ForceEnable()
    {
        if (GatherBuddy.Config.AlarmsEnabled)
        {
            Dalamud.ClientState.Login -= OnLogin;
            Dalamud.Framework.Update  += OnUpdate;
            SetActiveAlarms();
        }
    }

    public void Disable()
    {
        if (!GatherBuddy.Config.AlarmsEnabled)
            return;

        GatherBuddy.Config.AlarmsEnabled = false;
        GatherBuddy.Config.Save();
        Dalamud.Framework.Update -= OnUpdate;
        SetActiveAlarms();
        LastItemAlarm = null;
        LastFishAlarm = null;
    }

    public void SetDirty()
        => NextChange = TimeStamp.Epoch;

    public void Dispose()
        => Disable();

    private void SetActiveAlarms()
    {
        ActiveAlarms.Clear();
        if (!GatherBuddy.Config.AlarmsEnabled)
            return;

        foreach (var alarm in Alarms.Where(g => g.Enabled)
                     .SelectMany(g => g.Alarms)
                     .Where(a => a.Enabled))
            ActiveAlarms.TryAdd(alarm, TimeStamp.Epoch);
        SetDirty();
    }

    private void OnLogin(object? _, EventArgs _2)
        => SetDirty();
    
    private static bool AlarmActive(Alarm a, TimeInterval i)
        => GatherBuddy.Time.ServerTime >= i.Start.AddSeconds(-a.SecondOffset) && GatherBuddy.Time.ServerTime < i.End;

    public void OnUpdate(Framework _)
    {
        var st = GatherBuddy.Time.ServerTime;
        if (st < NextChange)
            return;

        if (Functions.BetweenAreas())
            return;

        if (!GatherBuddy.Config.AlarmsInDuty && Functions.BoundByDuty())
            return;

        var nextChange = st.AddDays(365);
        foreach (var (alarm, status) in ActiveAlarms)
        {
            var (location, uptime) = GatherBuddy.UptimeManager.BestLocation(alarm.Item);

            var shiftedStart = uptime.Start.AddSeconds(-alarm.SecondOffset);

            if (shiftedStart > st && uptime.Start < nextChange)
                nextChange = shiftedStart;

            if (uptime.End < nextChange)
                nextChange = uptime.End;

            if (uptime.Start == status)
                continue;

            ActiveAlarms[alarm] = uptime.Start;
            if (shiftedStart > st)
                continue;

            if (alarm.Item.Type == ObjectType.Fish)
                LastFishAlarm = (alarm, location, uptime);
            else if (alarm.Item.Type == ObjectType.Gatherable)
                LastItemAlarm = (alarm, location, uptime);

            if (alarm.SoundId > Sounds.Unknown)
                _sounds.Play(alarm.SoundId);

            alarm.SendMessage(location, uptime);
        }

        if (LastFishAlarm != null && !AlarmActive(LastFishAlarm.Value.Item1, LastFishAlarm.Value.Item3))
            LastFishAlarm = null;

        if (LastItemAlarm != null && !AlarmActive(LastItemAlarm.Value.Item1, LastItemAlarm.Value.Item3))
            LastItemAlarm = null;

        NextChange = nextChange;
    }

    private bool AddDefaultAlarms()
    {
        var def = Alarms.FirstOrDefault(a => a.Name == "Default");
        if (def != null)
            return false;

        def = new AlarmGroup()
        {
            Name        = "Default",
            Description = "Default alarm group, all new alarms from the item windows are added here first.",
        };
        Alarms.Insert(0, def);
        return true;
    }

    public void Save()
    {
        var file = Functions.ObtainSaveFile(FileName);
        if (file == null)
            return;

        try
        {
            var text = JsonConvert.SerializeObject(Alarms.Select(a => new AlarmGroup.Config(a)), Formatting.Indented);
            File.WriteAllText(file.FullName, text);
        }
        catch (Exception e)
        {
            PluginLog.Error($"Could not write gather groups to file {file.FullName}:\n{e}");
        }
    }

    public static AlarmManager Load()
    {
        var manager = new AlarmManager();
        var file    = Functions.ObtainSaveFile(FileName);
        if (file is not { Exists: true })
        {
            manager.AddDefaultAlarms();
            manager.Save();
            return manager;
        }

        try
        {
            var text    = File.ReadAllText(file.FullName);
            var data    = JsonConvert.DeserializeObject<AlarmGroup.Config[]>(text);
            var changes = false;
            foreach (var alarmGroup in data)
            {
                var group = new AlarmGroup()
                {
                    Name        = alarmGroup.Name,
                    Description = alarmGroup.Description,
                    Enabled     = alarmGroup.Enabled,
                };
                foreach (var item in alarmGroup.Alarms)
                {
                    if (!Alarm.FromConfig(item, out var alarm))
                    {
                        PluginLog.Error($"Could not add alarm to {@group.Name}.");
                        changes = true;
                        continue;
                    }

                    group.Alarms.Add(alarm!);
                }

                manager.Alarms.Add(group);
            }

            changes |= manager.AddDefaultAlarms();
            if (changes)
                manager.Save();
        }
        catch (Exception e)
        {
            PluginLog.Error($"Error loading gather groups:\n{e}");
            manager.AddDefaultAlarms();
            manager.Save();
        }

        return manager;
    }
}
