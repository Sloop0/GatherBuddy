using System.Collections.Generic;
using System.Linq;
using GatherBuddy.Classes;
using GatherBuddy.Enums;

namespace GatherBuddy.Managers;

public class NodeTimeLine
{
    public Dictionary<NodeType, Dictionary<GatheringType, List<GatheringNode>>> TimedNodesDict { get; }
    public GatheringNode[]                                                      TimedNodes     { get; }
    private class NodeComparer : IComparer<GatheringNode>
    {
        public int Compare(GatheringNode? lhs, GatheringNode? rhs)
            => (rhs?.Level ?? 0) - (lhs?.Level ?? 0);
    }

    public NodeTimeLine()
    {
        TimedNodesDict = new Dictionary<NodeType, Dictionary<GatheringType, List<GatheringNode>>>()
        {
            {
                NodeType.Unspoiled, new Dictionary<GatheringType, List<GatheringNode>>()
                {
                    { GatheringType.Botanist, new List<GatheringNode>() },
                    { GatheringType.Miner, new List<GatheringNode>() },
                }
            },
            {
                NodeType.Ephemeral, new Dictionary<GatheringType, List<GatheringNode>>()
                {
                    { GatheringType.Botanist, new List<GatheringNode>() },
                    { GatheringType.Miner, new List<GatheringNode>() },
                }
            },
        };

        foreach (var node in GatherBuddy.GameData.GatheringNodes.Values)
        {
            if (node.NodeType == NodeType.Regular)
                continue;
            if (node.GatheringType == GatheringType.Spearfishing)
                continue;

            TimedNodesDict[node.NodeType][node.GatheringType.ToGroup()].Add(node);
        }

        TimedNodesDict[NodeType.Unspoiled][GatheringType.Miner].Sort(new NodeComparer());
        TimedNodesDict[NodeType.Unspoiled][GatheringType.Botanist].Sort(new NodeComparer());
        TimedNodesDict[NodeType.Ephemeral][GatheringType.Miner].Sort(new NodeComparer());
        TimedNodesDict[NodeType.Ephemeral][GatheringType.Botanist].Sort(new NodeComparer());
    }

    private ShowNodes ExpansionForNode(GatheringNode n)
    {
        var addon = n.Territory.Data.ExVersion.Row;
        return addon switch
        {
            0 when n.Level <= 50    => ShowNodes.ARealmReborn,
            <= 1 when n.Level <= 60 => ShowNodes.Heavensward,
            <= 2 when n.Level <= 70 => ShowNodes.Stormblood,
            <= 3 when n.Level <= 80 => ShowNodes.Shadowbringers,
            <= 4 when n.Level <= 90 => ShowNodes.Endwalker,
            _                       => ShowNodes.AllNodes,
        };
    }

    public List<(GatheringNode, uint)> GetNewList(ShowNodes which)
    {
        var list = Enumerable.Empty<GatheringNode>();

        bool LevelCheck(GatheringNode n)
            => which.HasFlag(ExpansionForNode(n));

        if (which.HasFlag(ShowNodes.Unspoiled))
        {
            if (which.HasFlag(ShowNodes.Mining))
                list = list.Concat(TimedNodesDict[NodeType.Unspoiled][GatheringType.Miner]).Where(LevelCheck);
            if (which.HasFlag(ShowNodes.Botanist))
                list = list.Concat(TimedNodesDict[NodeType.Unspoiled][GatheringType.Botanist]).Where(LevelCheck);
        }

        if (which.HasFlag(ShowNodes.Ephemeral))
        {
            if (which.HasFlag(ShowNodes.Mining))
                list = list.Concat(TimedNodesDict[NodeType.Ephemeral][GatheringType.Miner]).Where(LevelCheck);
            if (which.HasFlag(ShowNodes.Botanist))
                list = list.Concat(TimedNodesDict[NodeType.Ephemeral][GatheringType.Botanist]).Where(LevelCheck);
        }

        return list.Select(n => (n, 25u)).ToList();
    }

    private class Comparer : IComparer<(GatheringNode, uint)>
    {
        public int Compare((GatheringNode, uint) lhs, (GatheringNode, uint) rhs)
        {
            if (lhs.Item2 != rhs.Item2)
                return (int)(lhs.Item2 - rhs.Item2);

            return rhs.Item1.Level - lhs.Item1.Level;
        }
    }

    private static void UpdateUptimes(int currentHour, IList<(GatheringNode, uint)> nodes)
    {
        for (var i = 0; i < nodes.Count; ++i)
            nodes[i] = (nodes[i].Item1, nodes[i].Item1.Times!.NextUptime(currentHour));
    }

    public static void SortByUptime(int currentHour, List<(GatheringNode, uint)> nodes)
    {
        UpdateUptimes(currentHour, nodes);
        nodes.Sort(new Comparer());
    }
}
