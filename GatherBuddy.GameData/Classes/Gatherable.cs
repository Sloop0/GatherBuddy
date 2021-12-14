using System;
using System.Collections.Generic;
using Dalamud.Data;
using Dalamud.Logging;
using GatherBuddy.Utility;
using Lumina.Excel.GeneratedSheets;

namespace GatherBuddy.Classes;

public class Gatherable : IComparable<Gatherable>
{
    public Item                   ItemData      { get; }
    public GatheringItem          GatheringData { get; }
    public MultiString            Name          { get; }
    public HashSet<GatheringNode> NodeList      { get; } = new();

    public uint ItemId
        => ItemData.RowId;

    public uint GatheringId
        => GatheringData.RowId;

    public Gatherable(GameData gameData, GatheringItem gatheringData)
    {
        GatheringData = gatheringData;
        var itemSheet = gameData.DataManager.GetExcelSheet<Item>();
        ItemData = itemSheet?.GetRow((uint)gatheringData.Item) ?? new Item();
        if (ItemData.RowId == 0)
            PluginLog.Error("Invalid item.");

        var levelData = gatheringData.GatheringItemLevel?.Value;
        _levelStars = levelData == null ? 0 : (levelData.GatheringItemLevel << 3) + levelData.Stars;
        Name        = MultiString.FromItem(gameData.DataManager, (uint) gatheringData.Item);
    }

    public int Level
        => _levelStars >> 3;

    public int Stars
        => _levelStars & 0b111;

    public string StarsString()
        => StarsArray[Stars];

    public override string ToString()
        => $"{Name} ({Level}{StarsString()})";

    public int CompareTo(Gatherable? rhs)
        => ItemId.CompareTo(rhs?.ItemId ?? 0);

    private readonly int _levelStars;

    private static readonly string[] StarsArray =
    {
        "",
        "*",
        "**",
        "***",
        "****",
    };
}
