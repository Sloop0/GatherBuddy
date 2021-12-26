using System;
using System.Collections.Generic;
using System.Diagnostics;
using Dalamud.Logging;
using GatherBuddy.Classes;
using GatherBuddy.SeFunctions;
using GatherBuddy.Time;

namespace GatherBuddy.Alarms;

public class Manager : IDisposable
{
    private readonly PlaySound _sounds;

    public  List<(IAlarm Alarm, bool Status)> Alarms = new();
    public  NodeAlarm?                        LastNodeAlarm { get; set; }
    public  FishAlarm?                        LastFishAlarm { get; set; }

    public Manager()
    {
        _sounds = new PlaySound(Dalamud.SigScanner);
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

    private static TimeStamp NextUptime(IAlarm alarm)
    {
        
        switch (alarm)
        {
            case NodeAlarm n:
               return n.Node.Times.NextUptime()
            case FishAlarm f: 
                return f.Fish.NextUptime().Start < time;
        }
        Debug.Assert(false, "Can not be reached.");
        return false;
    }

    public void OnUpdate()
    {
        // Skip if the player isn't loaded in a territory.
        if (Dalamud.ClientState.TerritoryType == 0 || Dalamud.ClientState.LocalPlayer == null)
            return;

        for (var i = 0; i < Alarms.Count; ++i)
        {
            var (alarm, status) = Alarms[i];
            if (!alarm.Enabled)
                continue;

            var currentTime = SeTime.ServerTime.AddSeconds(alarm.SecondOffset);
            var nextTime    = 
            var newStatus   = NewStatus(alarm);

            if (status == newStatus)
                continue;

            Alarms[i] = (alarm, newStatus);
            if (newStatus)
                Ring(alarm);
        }
    }

    private void Ring(IAlarm alarm)
    {
        if (alarm.SoundId > Sounds.Unknown)
            _sounds.Play(alarm.SoundId);

        alarm.SendMessage();
        if (alarm.PrintMessage)
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

        switch (alarm)
        {
            case NodeAlarm n:
                LastNodeAlarm = n;
                break;
            case FishAlarm f:
                LastFishAlarm = f;
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
