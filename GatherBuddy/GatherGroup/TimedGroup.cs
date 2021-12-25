using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.Gui;
using GatherBuddy.Time;
using Newtonsoft.Json;

namespace GatherBuddy.GatherGroup;

public class TimedGroupConverter : JsonConverter<TimedGroup>
{
    public override void WriteJson(JsonWriter writer, TimedGroup? value, JsonSerializer serializer)
    {
        if (value == null)
            writer.WriteToken(JsonToken.Null);

        writer.WriteValue(value);
    }

    public override TimedGroup ReadJson(JsonReader reader, Type objectType, TimedGroup? existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
    }
}

public class TimedGroup
{
    public string Name        { get; }
    public string Description { get; private set; } = string.Empty;

    public TimedGroup(string name)
        => Name = name;

    public List<TimedGroupNode> Nodes { get; init; } = new();

    public TimedGroupNode? CurrentNode(uint eorzeaMinuteOfDay)
        => Nodes.FirstOrDefault(node => node.IsUp(eorzeaMinuteOfDay));
};

public class TimedGroupManager
{
    public Dictionary<string, TimedGroup> Groups { get; init; } = new();

    public bool AddGroup(string name)
    {
        var lowerName = name.ToLowerInvariant();
        if (Groups.ContainsKey(lowerName))
            return false;

        var newConfig = new TimedGroupConfig { Name   = name };
        Groups.Add(lowerName, new TimedGroup { Config = newConfig });
        return true;
    }

    public static bool ChangeDescription(string description, TimedGroupConfig config)
    {
        if (config.Description == description)
            return false;

        config.Description = description;
        return true;
    }


    public static void PrintHelp(ChatGui chat, Dictionary<string, TimedGroup> groups)
    {
        chat.Print("Use with [GroupName] [optional:minute offset], valid GroupNames are:");
        foreach (var (key, value) in groups)
            chat.Print($"        {key} - {value.Description}");
    }
}
