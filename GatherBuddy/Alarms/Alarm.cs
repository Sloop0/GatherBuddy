using System;
using GatherBuddy.Caching;
using GatherBuddy.Classes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GatherBuddy.Alarms;

public enum AlarmType : byte
{
    Node,
    Fish,
}

public interface IAlarm
{
    public AlarmType Type         { get; }
    public uint      Id           { get; }
    public string    Name         { get; set; }
    public int       SecondOffset { get; set; }
    public Sounds    SoundId      { get; set; }
    public bool      Enabled      { get; set; }
    public bool      PrintMessage { get; set; }
}

public class FishAlarm : IAlarm
{
    public AlarmType Type
        => AlarmType.Fish;

    public ExtendedFish Fish { get; }

    public uint Id
        => Fish.Fish.ItemId;

    public string    Name         { get; set; }
    public int       SecondOffset { get; set; }
    public Sounds    SoundId      { get; set; }
    public bool      Enabled      { get; set; }
    public bool      PrintMessage { get; set; }
}

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
}

public class Alarm
{
    [JsonIgnore]
    private object? _data;

    [JsonIgnore]
    public GatheringNode? Node
        => _data as GatheringNode;

    [JsonIgnore]
    public ExtendedFish? Fish
        => _data as ExtendedFish;

    public string Name         { get; set; } = string.Empty;

    public uint Id
    {
        get => Node?.BaseId ?? Fish?.Fish.ItemId ?? 0;
        set
        {
            if (Type == AlarmType.Node)
                _data = GatherBuddy.GameData.GatheringNodes.TryGetValue(value, out var node)
                    ? node
                    : throw new ArgumentException($"Invalid node {value}.");
            else
                _data = GatherBuddy.GameData.Fishes.TryGetValue(value, out var fish)
                    ? fish
                    : throw new ArgumentException($"Invalid fish {value}.");
        }
    }

    public int    MinuteOffset { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public AlarmType Type { get; set; } = AlarmType.Node;

    public Sounds SoundId      { get; set; }
    public bool   Enabled      { get; set; }
    public bool   PrintMessage { get; set; }

    // backward compatibility.
    public uint NodeId
    {
        set => Id = value;
    }

    public Alarm()
    { }

    public Alarm(AlarmType type, string name, uint id, bool enabled = true, int offset = 0, Sounds sound = Sounds.None,
        bool printMessage = true)
    {
        Name         = name;
        NodeId       = id;
        MinuteOffset = offset;
        SoundId      = sound;
        PrintMessage = printMessage;
        Enabled      = enabled;
        Type         = type;
    }

    public Alarm(string name, GatheringNode? node, bool enabled = true, int offset = 0, Sounds sound = Sounds.None, bool printMessage = true)
        : this(AlarmType.Node, name, node?.BaseId ?? 0, enabled, offset, sound, printMessage)
        => _data = node;

    public static Alarm FromNodeId(string name, uint nodeId, bool enabled = true, int offset = 0, Sounds sound = Sounds.None,
        bool printMessage = true)
    {
        var alarm = new Alarm(AlarmType.Node, name, nodeId, enabled, offset, sound, printMessage)
        {
            _data = GatherBuddy.GameData.GatheringNodes.TryGetValue(nodeId, out var node) ? node : null,
        };
        return alarm;
    }

    public Alarm(string name, Fish? fish, bool enabled = true, int offset = 0, Sounds sound = Sounds.None, bool printMessage = true)
        : this(AlarmType.Fish, name, fish?.ItemId ?? 0, enabled, offset, sound, printMessage)
        => _data = fish;

    public static Alarm FromFishId(string name, uint fishId, bool enabled = true, int offset = 0, Sounds sound = Sounds.None,
        bool printMessage = true)
    {
        var alarm = new Alarm(AlarmType.Fish, name, fishId, enabled, offset, sound, printMessage)
        {
            _data = GatherBuddy.GameData.Fishes.TryGetValue(fishId, out var fish) ? fish : null,
        };
        return alarm;
    }
}
