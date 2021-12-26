using Dalamud.Logging;
using GatherBuddy.Caching;

namespace GatherBuddy.Alarms;

public class FishAlarm : IAlarm
{
    public AlarmType Type
        => AlarmType.Fish;

    public ExtendedFish Fish { get; }

    public uint Id
        => Fish.Fish.ItemId;

    public string Name         { get; set; }
    public int    SecondOffset { get; set; } = 0;
    public Sounds SoundId      { get; set; } = Sounds.None;
    public bool   Enabled      { get; set; } = false;
    public bool   PrintMessage { get; set; } = true;

    public FishAlarm(ExtendedFish fish, string name = "")
    {
        Fish = fish;
        Name = name;
    }

    public void SendMessage(long timeDiff)
    {
        if (PrintMessage && GatherBuddy.Config.FishAlarmFormat.Length > 0)
            Dalamud.Chat.PrintError(Communicator.FormatFishAlarmMessage(GatherBuddy.Config.FishAlarmFormat, this, timeDiff));
        PluginLog.Verbose(Communicator.FormatFishAlarmMessage(GatherBuddyConfiguration.DefaultFishAlarmFormat, this, timeDiff).ToString());
    }
}
