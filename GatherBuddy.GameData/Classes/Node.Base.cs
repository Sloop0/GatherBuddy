using System;
using System.Linq;
using Dalamud.Interface.ImGuiFileDialog;
using GatherBuddy.Enums;
using GatherBuddy.Time;
using GatherBuddy.Utility;
using Lumina.Excel.GeneratedSheets;
using GatheringType = GatherBuddy.Enums.GatheringType;

namespace GatherBuddy.Classes;

public partial class GatheringNode : IComparable
{
    public NodeType           NodeType     { get; init; } = NodeType.Regular;
    public GatheringPointBase BaseNodeData { get; init; }
    public BitfieldUptime     Times        { get; init; } = BitfieldUptime.AllHours;

    public uint BaseId
        => BaseNodeData.RowId;

    public int Level
        => BaseNodeData.GatheringLevel;

    public GatheringType GatheringType
        => (GatheringType)BaseNodeData.GatheringType.Row;

    public bool IsMiner
        => GatheringType.ToGroup() == GatheringType.Miner;

    public bool IsBotanist
        => GatheringType.ToGroup() == GatheringType.Botanist;

    public GatheringNode(GameData data, GatheringPointBase node)
    {
        BaseNodeData = node;

        // Obtain the territory from the first node that has this as a base.
        var nodes   = data.DataManager.GetExcelSheet<GatheringPoint>();
        var nodeRow = nodes?.FirstOrDefault(n => n.GatheringPointBase.Row == node.RowId);
        Territory = data.FindOrAddTerritory(nodeRow?.TerritoryType.Value) ?? Territory.Invalid;

        // Obtain the center of the coordinates. We do not care for the radius.
        var coords = data.DataManager.GetExcelSheet<ExportedGatheringPoint>();
        var coordRow    = coords?.GetRow(node.RowId);
        if (coordRow != null)
        {
            IntegralXCoord = Maps.NodeToMap(coordRow.X, Territory.SizeFactor);
            IntegralYCoord = Maps.NodeToMap(coordRow.Y, Territory.SizeFactor);
        }
        ClosestAetheryte = Territory.Aetherytes.Count > 0 
            ? Territory.Aetherytes.ArgMin(a => a.WorldDistance(Territory.Id, IntegralXCoord, IntegralYCoord)) 
            : null;

        // Obtain the items and add the nodes to their individual lists.
        Items = node.Item
            .Select(i => data.Gatherables.TryGetValue((uint) i, out var gatherable) ? gatherable : null)
            .Where(g => g != null)
            .Cast<Gatherable>()
            .ToList();
        foreach (var item in Items)
            item.NodeList.Add(this);
    }

    public int CompareTo(object? obj)
    {
        var rhs = obj as GatheringNode;
        return BaseId.CompareTo(rhs?.BaseId ?? 0);
    }
}
