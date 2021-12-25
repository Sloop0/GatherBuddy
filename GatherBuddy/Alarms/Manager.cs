using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Logging;
using GatherBuddy.Caching;
using GatherBuddy.Classes;
using GatherBuddy.Enums;
using GatherBuddy.SeFunctions;
using GatherBuddy.Time;

namespace GatherBuddy.Alarms;

public class Manager : IDisposable
{
    private readonly PlaySound _sounds;

    public List<Alarm> Alarms
        => GatherBuddy.Config.Alarms;

    private readonly List<bool> _status;
    private          int        _lastEorzeaMinute;
    public           Alarm?     LastNodeAlarm { get; set; }
    public           Alarm?     LastFishAlarm { get; set; }

    public Manager()
    {
        _sounds = new PlaySound(Dalamud.SigScanner);
        _status = Enumerable.Repeat(false, Alarms.Count).ToList();
    }

    public void Dispose()
    {
        if (GatherBuddy.Config.AlarmsEnabled)
            SeTime.Updated -= OnUpdate;
    }

    public void Enable(bool force = false)
    {
        if (!force && GatherBuddy.Config.AlarmsEnabled)
            return;

        GatherBuddy.Config.AlarmsEnabled =  true;
        SeTime.Updated                   += OnUpdate;
        GatherBuddy.Config.Save();
    }

    public void Disable()
    {
        if (!GatherBuddy.Config.AlarmsEnabled)
            return;

        GatherBuddy.Config.AlarmsEnabled =  false;
        SeTime.Updated                   -= OnUpdate;
        GatherBuddy.Config.Save();
    }

    private static bool NewStatusNode(Alarm nodeAlarm)
    {
        var hour      = (SeTime.EorzeaMinuteOfDay + nodeAlarm.MinuteOffset) / RealTime.MinutesPerHour;
        var hourOfDay = (uint)hour % RealTime.HoursPerDay;
        return nodeAlarm.Node!.Times.IsUp(hourOfDay);
    }

    private static bool NewStatusFish(Alarm fishAlarm)
    {
        var uptime = fishAlarm.Fish!.NextUptime();
        return uptime.Start - SeTime.ServerTime < fishAlarm.MinuteOffset * RealTime.SecondsPerMinute;
    }

    public void OnUpdate()
    {
        // Skip if the player isn't loaded in a territory.
        if (Dalamud.ClientState.TerritoryType == 0 || Dalamud.ClientState.LocalPlayer == null)
            return;

        var minute = SeTime.EorzeaMinuteOfDay;
        if (minute == _lastEorzeaMinute)
            return;

        _lastEorzeaMinute = minute;
        for (var i = 0; i < Alarms.Count; ++i)
        {
            var alarm = Alarms[i];
            if (!alarm.Enabled)
                continue;

            var newStatus = alarm.Type switch
            {
                AlarmType.Node => NewStatusNode(alarm),
                AlarmType.Fish => NewStatusFish(alarm),
                _              => false,
            };

            if (_status[i] == newStatus)
                continue;

            _status[i] = newStatus;
            if (newStatus)
                Ring(alarm);
        }
    }

    private void Ring(Alarm alarm)
    {
        if (alarm.SoundId > Sounds.Unknown)
            _sounds.Play(alarm.SoundId);

        if (alarm.PrintMessage)
        {
            switch (alarm.Type)
            {
                case AlarmType.Node when GatherBuddy.Config.NodeAlarmFormat.Length > 0:
                    Dalamud.Chat.PrintError(Communicator.FormatAlarmMessage(GatherBuddy.Config.NodeAlarmFormat, alarm,
                        (uint)SeTime.EorzeaMinuteOfDay));
                    PluginLog.Verbose(Communicator
                        .FormatAlarmMessage(GatherBuddyConfiguration.DefaultNodeAlarmFormat, alarm, (uint)SeTime.EorzeaMinuteOfDay)
                        .ToString());
                    break;
                case AlarmType.Fish when GatherBuddy.Config.FishAlarmFormat.Length > 0:
                    Dalamud.Chat.PrintError(Communicator.FormatAlarmMessage(GatherBuddy.Config.FishAlarmFormat, alarm,
                        (uint)SeTime.EorzeaMinuteOfDay));
                    PluginLog.Verbose(Communicator
                        .FormatAlarmMessage(GatherBuddyConfiguration.DefaultFishAlarmFormat, alarm, (uint)SeTime.EorzeaMinuteOfDay).ToString());
                    break;
            }
        }

        switch (alarm.Type)
        {
            case AlarmType.Node:
                LastNodeAlarm = alarm;
                break;
            case AlarmType.Fish:
                LastFishAlarm = alarm;
                break;
        }
    }

    public void AddNode(string name, uint nodeId)
    {
        var alarm = Alarm.FromNodeId(name, nodeId);
        if (alarm.Node == null)
            return;

        Alarms.Add(alarm);
        _status.Add(false);
        GatherBuddy.Config.Save();
    }

    public void AddFish(string name, uint fishId)
    {
        var alarm = Alarm.FromFishId(name, fishId);
        if (alarm.Fish == null)
            return;

        Alarms.Add(alarm);
        _status.Add(false);
        GatherBuddy.Config.Save();
    }

    public void AddNode(string name, GatheringNode node)
    {
        Alarms.Add(new Alarm(name, node));
        _status.Add(false);
        GatherBuddy.Config.Save();
    }

    public void AddFish(string name, Fish fish)
    {
        Alarms.Add(new Alarm(name, fish));
        _status.Add(false);
        GatherBuddy.Config.Save();
    }

    public void RemoveAlarm(int idx)
    {
        if (idx >= Alarms.Count)
            return;

        Alarms.RemoveAt(idx);
        _status.RemoveAt(idx);
        GatherBuddy.Config.Save();
    }

    public void ChangeNodeSound(int idx, Sounds sound)
    {
        if (idx >= Alarms.Count)
            return;

        Alarms[idx].SoundId = sound;
        GatherBuddy.Config.Save();
    }

    public void ChangeNodeOffset(int idx, int offset)
    {
        if (idx >= Alarms.Count)
            return;

        Alarms[idx].MinuteOffset = offset;
        GatherBuddy.Config.Save();
    }

    public void ChangeNodeStatus(int idx, bool enabled)
    {
        if (idx >= Alarms.Count)
            return;

        Alarms[idx].Enabled = enabled;
        GatherBuddy.Config.Save();
    }

    public void ChangePrintStatus(int idx, bool print)
    {
        if (idx >= Alarms.Count)
            return;

        Alarms[idx].PrintMessage = print;
        GatherBuddy.Config.Save();
    }
}
