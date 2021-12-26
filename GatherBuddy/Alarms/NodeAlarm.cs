using Dalamud.Logging;
using GatherBuddy.Classes;

namespace GatherBuddy.Alarms;

public class NodeAlarm : IAlarm
{
    public AlarmType Type
        => AlarmType.Node;

    public GatheringNode Node { get; }

    public uint Id
        => Node.BaseId;

    public string Name         { get; set; }
    public int    SecondOffset { get; set; }
    public Sounds SoundId      { get; set; }
    public bool   Enabled      { get; set; }
    public bool   PrintMessage { get; set; }

    public NodeAlarm(GatheringNode node, string name = "")
    {
        Node = node;
        Name = name;
    }

    public void SendMessage(long timeDiff)
    {
        if (PrintMessage && GatherBuddy.Config.NodeAlarmFormat.Length > 0)
            Dalamud.Chat.PrintError(Communicator.FormatNodeAlarmMessage(GatherBuddy.Config.NodeAlarmFormat, this, timeDiff));
        PluginLog.Verbose(Communicator.FormatNodeAlarmMessage(GatherBuddyConfiguration.DefaultNodeAlarmFormat, this, timeDiff).ToString());
    }
}
