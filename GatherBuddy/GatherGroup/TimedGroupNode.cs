using System;
using GatherBuddy.Interfaces;
using GatherBuddy.Time;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GatherBuddy.GatherGroup;

public class TimedGroupNode
{
    public IGatherable Item;
    public int         EorzeaStartMinute;
    public int         EorzeaEndMinute;
    public string      Annotation = string.Empty;

    public TimedGroupNode(IGatherable item)
        => Item = item;

    public bool IsUp(uint eorzeaMinuteOfDay)
    {
        if (EorzeaStartMinute <= EorzeaEndMinute)
            return eorzeaMinuteOfDay >= EorzeaStartMinute && eorzeaMinuteOfDay < EorzeaEndMinute;

        return eorzeaMinuteOfDay >= EorzeaStartMinute || eorzeaMinuteOfDay < EorzeaEndMinute;
    }

    public TimedGroupNode Clone()
        => new(Item)
        {
            EorzeaStartMinute = EorzeaStartMinute,
            EorzeaEndMinute   = EorzeaEndMinute,
            Annotation        = Annotation,
        };

    internal struct Config
    {
        public uint ItemId;

        [JsonConverter(typeof(StringEnumConverter))]
        public ObjectType Type;

        public int    StartMinute;
        public int    EndMinute;
        public string Annotation;
    }

    internal Config ToConfig()
        => new()
        {
            ItemId      = Item.ItemId,
            Type        = Item.Type,
            StartMinute = EorzeaStartMinute,
            EndMinute   = EorzeaEndMinute,
            Annotation  = Annotation,
        };

    internal static bool FromConfig(Config cfg, out TimedGroupNode? timedGroupNode)
    {
        timedGroupNode = null;
        IGatherable? item = cfg.Type switch
        {
            ObjectType.Gatherable => GatherBuddy.GameData.Gatherables.TryGetValue(cfg.ItemId, out var i) ? i : null,
            ObjectType.Fish       => GatherBuddy.GameData.Fishes.TryGetValue(cfg.ItemId, out var f) ? f : null,
            _                     => null,
        };
        if (item == null)
            return false;

        timedGroupNode = new TimedGroupNode(item)
        {
            EorzeaStartMinute = Math.Clamp(cfg.StartMinute, 0, RealTime.MinutesPerDay),
            EorzeaEndMinute   = Math.Clamp(cfg.EndMinute,   0, RealTime.MinutesPerDay),
            Annotation        = cfg.Annotation,
        };
        return true;
    }
}
