using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dalamud.Logging;
using GatherBuddy.GatherGroup;
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
    internal Dictionary<Alarm, bool>           ActiveAlarms  { get; init; } = new();
    public   (Alarm, ILocation, TimeInterval)? LastItemAlarm { get; private set; }
    public   (Alarm, ILocation, TimeInterval)? LastFishAlarm { get; private set; }

    private AlarmGroup? _alarmGroup = null;
    public  AlarmGroup  DefaultGroup
        => _alarmGroup ??= AddDefaultAlarms();

    public AlarmManager()
        => _sounds = new PlaySound(Dalamud.SigScanner);

    public void Dispose()
    { }

    public void OnUpdate()
    {
        // Skip if the player isn't loaded in a territory.
        if (Dalamud.ClientState.TerritoryType == 0 || Dalamud.ClientState.LocalPlayer == null)
            return;

        foreach (var (alarm, status) in ActiveAlarms)
        {
            var time = GatherBuddy.Time.ServerTime.AddSeconds(alarm.SecondOffset);
            var (location, uptime) = GatherBuddy.UptimeManager.BestLocation(alarm.Item);
            if (uptime.End > time)
                ActiveAlarms[alarm] = false;
            if (uptime.End > time || status)
                continue;

            ActiveAlarms[alarm] = true;
            if (alarm.Item.Type == ObjectType.Fish)
                LastFishAlarm = (alarm, location, uptime);
            else if (alarm.Item.Type == ObjectType.Gatherable)
                LastItemAlarm = (alarm, location, uptime);

            if (alarm.SoundId > Sounds.Unknown)
                _sounds.Play(alarm.SoundId);

            alarm.SendMessage(location, uptime);
        }
    }

    private AlarmGroup AddDefaultAlarms()
    {
        var def = Alarms.FirstOrDefault(a => a.Name == "Default");
        if (def != null)
            return def;

        def = new AlarmGroup()
        {
            Name        = "Default",
            Description = "Default alarm group, all new alarms from the item windows are added here first.",
        };
        Alarms.Insert(0, def);
        return def;
    }

    public void Save()
    {
        var file = Functions.ObtainSaveFile(FileName);
        if (file == null)
            return;
    
        try
        {
            //var text = JsonConvert.SerializeObject(Alarms.Select(a => ), Formatting.Indented);
            //File.WriteAllText(file.FullName, text);
        }
        catch (Exception e)
        {
            PluginLog.Error($"Could not write gather groups to file {file.FullName}:\n{e}");
        }
    }
    //public static AlarmManager Load()
    //{
    //    var manager = new AlarmManager();
    //    var file    = Utility.Functions.ObtainSaveFile(FileName);
    //    if (file is not { Exists: true })
    //    {
    //        manager.Save();
    //        return manager;
    //    }
    //
    //    try
    //    {
    //        var text    = File.ReadAllText(file.FullName);
    //        var data    = JsonConvert.DeserializeObject<List<TimedGroup.Config>>(text);
    //        var changes = false;
    //        foreach (var config in data)
    //        {
    //            if (!TimedGroup.FromConfig(config, out var group))
    //            {
    //                PluginLog.Error($"Invalid items in gather group {group.Name} skipped.");
    //                changes = true;
    //            }
    //
    //            var searchName = group.Name.ToLowerInvariant().Trim();
    //            if (searchName.Length == 0)
    //            {
    //                changes = true;
    //                PluginLog.Error("Gather group without name found, skipping.");
    //                continue;
    //            }
    //
    //            if (manager.Groups.ContainsKey(searchName))
    //            {
    //                changes = true;
    //                PluginLog.Error($"Multiple gather groups with the same name {searchName} found, skipping later ones.");
    //                continue;
    //            }
    //
    //            manager.Groups.Add(searchName, group);
    //        }
    //
    //        changes |= manager.SetDefaults();
    //        if (changes)
    //            manager.Save();
    //    }
    //    catch (Exception e)
    //    {
    //        PluginLog.Error($"Error loading gather groups:\n{e}");
    //        manager.Groups.Clear();
    //        manager.SetDefaults();
    //        manager.Save();
    //    }
    //
    //    return manager;
    //}
}
