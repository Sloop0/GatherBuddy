using System;
using System.Collections.Generic;
using System.Linq;
using GatherBuddy.Classes;
using GatherBuddy.Managers;
using GatherBuddy.Time;
using GatherBuddy.Utility;
using ImGuiNET;

namespace GatherBuddy.Gui.Cache;

internal class NodeTab
{
    private readonly NodeTimeLine                   _nodeTimeLine;
    private readonly Dictionary<GatheringNode, string> _allNodeItems;

    public List<(GatheringNode, uint)>       ActiveNodes;
    public (GatheringNode, string, string)[] ActiveNodeItems;
    public int                               HourOfDay;
    public long                              TotalHour;

    public readonly float BotanistTextSize;
    public readonly float NodeTypeTextSize;

    public string NodeFilter      = "";
    public string NodeFilterLower = "";

    private void UpdateNodes()
        => ActiveNodeItems = ActiveNodes.Select(n => (n.Item1, _allNodeItems[n.Item1], _allNodeItems[n.Item1].ToLowerInvariant()))
            .ToArray();

    public NodeTab(NodeTimeLine nodeTimeLine)
    {
        _nodeTimeLine   = nodeTimeLine;
        ActiveNodes     = _nodeTimeLine.GetNewList(GatherBuddy.Config.ShowNodes);
        ActiveNodeItems = Array.Empty<(GatheringNode, string, string)>();
        HourOfDay       = TimeStamp.UtcNow.CurrentEorzeaHour();
        _allNodeItems = _nodeTimeLine.GetNewList(ShowNodes.AllNodes)
            .ToDictionary(n => n.Item1, n => n.Item1.PrintItems(", ", GatherBuddy.Language));
        UpdateNodes();
        TotalHour = 0;

        BotanistTextSize = ImGui.CalcTextSize("Botanist").X / ImGui.GetIO().FontGlobalScale;
        NodeTypeTextSize = System.Math.Max(ImGui.CalcTextSize("Unspoiled").X, ImGui.CalcTextSize("Ephemeral").X) / ImGui.GetIO().FontGlobalScale;
    }

    public bool Rebuild()
    {
        HourOfDay   = TimeStamp.UtcNow.CurrentEorzeaHour();
        ActiveNodes = _nodeTimeLine.GetNewList(GatherBuddy.Config.ShowNodes);
        if (ActiveNodes.Count > 0)
            NodeTimeLine.SortByUptime(HourOfDay, ActiveNodes);
        UpdateNodes();
        return true;
    }

    public void Update(long totalHour)
    {
        if (ActiveNodes.Count == 0 || totalHour <= TotalHour)
            return;

        TotalHour = totalHour;
        HourOfDay = (int)totalHour % RealTime.HoursPerDay;
        NodeTimeLine.SortByUptime(HourOfDay, ActiveNodes);
        UpdateNodes();
    }
}
