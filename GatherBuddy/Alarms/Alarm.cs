using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Logging;
using GatherBuddy.Interfaces;
using GatherBuddy.Time;

namespace GatherBuddy.Alarms;

public class Alarm
{
    public IGatherable Item         { get; set; }
    public string      Name         { get; set; } = string.Empty;
    public int         SecondOffset { get; set; }
    public Sounds      SoundId      { get; set; }
    public bool        Enabled      { get; set; }
    public bool        PrintMessage { get; set; }

    public Alarm(IGatherable item)
        => Item = item;

    public Alarm Clone()
        => new(Item)
        {
            Name         = Name,
            SecondOffset = SecondOffset,
            SoundId      = SoundId,
            Enabled      = false,
            PrintMessage = PrintMessage,
        };

    public void SendMessage(ILocation location, TimeInterval uptime)
    {
        if (!PrintMessage)
            return;

        switch (Item.Type)
        {
            //case ObjectType.Invalid:
            //    PluginLog.Error($"Invalid item for Alarm {Name}.");
            //    break;
            //case ObjectType.Gatherable:
            //    if (PrintMessage && GatherBuddy.Config.NodeAlarmFormat.Length > 0)
            //        Dalamud.Chat.PrintError(Communicator.FormatNodeAlarmMessage(GatherBuddy.Config.NodeAlarmFormat, this, timeDiff));
            //    PluginLog.Verbose(Communicator.FormatNodeAlarmMessage(GatherBuddyConfiguration.DefaultNodeAlarmFormat, this, timeDiff)
            //        .ToString());
            //    break;
            //case ObjectType.Fish:
            //    if (PrintMessage && GatherBuddy.Config.FishAlarmFormat.Length > 0)
            //        Dalamud.Chat.PrintError(Communicator.FormatFishAlarmMessage(GatherBuddy.Config.FishAlarmFormat, this, timeDiff));
            //    PluginLog.Verbose(Communicator.FormatFishAlarmMessage(GatherBuddyConfiguration.DefaultFishAlarmFormat, this, timeDiff)
            //        .ToString());
            //    break;
            //default: throw new ArgumentOutOfRangeException();
        }
    }

    internal struct Config
    {
        public uint       Id;
        public ObjectType Type;
        public int        SecondOffset;
        public Sounds     SoundId;
        public bool       Enabled;
        public bool       PrintMessage;
    }

    internal static bool FromConfig(Config config, out Alarm? alarm)
    {
        alarm = null;
        return false;
    }
}

public class AlarmGroup
{
    public string      Name        { get; set; } = string.Empty;
    public string      Description { get; set; } = string.Empty;
    public List<Alarm> Alarms      { get; set; } = new();
    public bool        Enabled     { get; set; } = false;

    public AlarmGroup Clone()
        => new()
        {
            Name        = Name,
            Description = Description,
            Alarms      = Alarms.Select(a => a.Clone()).ToList(),
            Enabled     = false,
        };
}
