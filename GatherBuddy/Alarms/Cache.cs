using System;
using System.Linq;
using GatherBuddy.Classes;
using ImGuiNET;

namespace GatherBuddy.Alarms;

//internal class Cache
//{
//    public readonly GatheringNode[] AllTimedNodes;
//    public readonly ExtendedFish[]  AllTimedFish;
//    public readonly string[]        AllTimedNodeNames;
//    public readonly string[]        SoundNames;
//    public readonly float           LongestNodeNameLength;
//    public readonly Manager         Manager;
//
//    public const float NameSize   = 135;
//    public const float SoundSize  = 85;
//    public const float OffsetSize = 35;
//
//    public string NewName;
//    public string NodeFilter;
//    public int    NewIdx;
//    public bool   FocusFilter;
//
//    public Cache(Manager manager, NodeTimeLine nodes, FishManager fish)
//    {
//        Manager       = manager;
//        AllTimedNodes = nodes.TimedNodes;
//        AllTimedFish  = fish.TimedFish;
//        SoundNames    = Enum.GetNames(typeof(Sounds)).Where(s => s != "Unknown").ToArray();
//
//        AllTimedNodeNames = AllTimedNodes
//            .Select(n => $"{n.Times!.PrintHours(true)}: {n.PrintItems(", ", GatherBuddy.Language)}")
//            .Concat(AllTimedFish.Select(f => f.Name))
//            .ToArray();
//        LongestNodeNameLength = AllTimedNodeNames.Max(n => ImGui.CalcTextSize(n).X) / ImGui.GetIO().FontGlobalScale;
//
//        NewName     = "";
//        NewIdx      = 0;
//        FocusFilter = false;
//        NodeFilter  = "";
//    }
//
//    public void AddAlarm()
//    {
//        if (NewIdx < AllTimedNodes.Length)
//            Manager.AddNode(NewName, AllTimedNodes[NewIdx]);
//        else
//            Manager.AddFish(NewName, AllTimedFish[NewIdx - AllTimedNodes.Length].Fish);
//
//        NewName = "";
//    }
//}
