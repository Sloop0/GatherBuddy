using System;
using Dalamud.Logging;
using GatherBuddy.Classes;
using GatherBuddy.Time;
using Newtonsoft.Json;
using JsonException = Newtonsoft.Json.JsonException;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace GatherBuddy.GatherGroup;

public class GatheringNodeConverter : JsonConverter<GatheringNode>
{
    public override void WriteJson(JsonWriter writer, GatheringNode? value, JsonSerializer serializer)
        => writer.WriteValue((value?.BaseId ?? 0).ToString());

    public override GatheringNode ReadJson(JsonReader reader, Type objectType, GatheringNode? existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        var id = reader.TokenType == JsonToken.Integer ? (uint)reader.Value! : uint.TryParse(reader.Value as string, out var val) ? val : 0;
        if (GatherBuddy.GameData.GatheringNodes.TryGetValue(id, out var node))
            return node;

        return hasExistingValue ? existingValue! : throw new JsonException("Could not parse node id.");
    }
}

public class GatherableConverter : JsonConverter<Gatherable>
{
    public override void WriteJson(JsonWriter writer, Gatherable? value, JsonSerializer serializer)
        => writer.WriteValue(value?.ItemId.ToString() ?? "NULL");

    public override Gatherable ReadJson(JsonReader reader, Type objectType, Gatherable? existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        var id = reader.TokenType == JsonToken.Integer ? (uint)reader.Value! : uint.TryParse(reader.Value as string, out var val) ? val : 0;
        if (GatherBuddy.GameData.Gatherables.TryGetValue(id, out var item))
            return item;

        return hasExistingValue ? existingValue! : throw new JsonException("Could not parse item id.");
    }
}

public class TimedGroupNode
{
    [JsonConverter(typeof(GatheringNodeConverter))]
    public GatheringNode Node;
    [JsonConverter(typeof(GatherableConverter))]
    public Gatherable?   Item;
    public int           EorzeaStartMinute;
    public int           EorzeaEndMinute;
    public string        Annotation = string.Empty;

    public bool IsUp(uint eorzeaMinuteOfDay)
    {
        if (EorzeaStartMinute <= EorzeaEndMinute)
            return eorzeaMinuteOfDay >= EorzeaStartMinute && eorzeaMinuteOfDay < EorzeaEndMinute;
        return eorzeaMinuteOfDay >= EorzeaStartMinute || eorzeaMinuteOfDay < EorzeaEndMinute;
    }

    public static TimedGroupNode? FromConfig(TimedGroupNodeConfig group)
    {
        if (!GatherBuddy.GameData.GatheringNodes.TryGetValue(group.NodeId, out var node))
        {
            PluginLog.Error($"Could not create timed group node due to missing gathering node {group.NodeId}.");
            return null;
        }

        var interval = RepeatingInterval.FromEorzeanMinutes((int)group.StartMinute, (int)group.EndMinute);
        if (interval == RepeatingInterval.Never)
            PluginLog.Warning($"Timed Group Node has no active time slot.");

        Gatherable? item = null;
        if (group.ItemId != 0)
        {
            if (!GatherBuddy.GameData.Gatherables.TryGetValue(group.ItemId, out var tmp))
            {
                PluginLog.Error($"Could not create timed group node due to missing item {group.ItemId}.");
                return null;
            }

            if (!node.HasItems(tmp))
            {
                PluginLog.Error(
                    $"Could not create timed group node because the item {tmp.Name.English} is not available for that node.");
                return null;
            }

            item = tmp;
        }

        return new TimedGroupNode
        {
            Node       = node,
            Interval   = interval,
            Item       = item,
            Annotation = group.Annotation,
        };
    }
}
