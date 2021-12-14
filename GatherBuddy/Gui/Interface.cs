using System;
using System.Globalization;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using GatherBuddy.Classes;
using GatherBuddy.Game;
using GatherBuddy.Levenshtein;
using GatherBuddy.Managers;
using GatherBuddy.Utility;
using GatherBuddyA;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace GatherBuddy.Gui;

public partial class Interface : IDisposable
{
    private const string PluginName             = "GatherBuddy";
    private const float  DefaultHorizontalSpace = 5;

    private readonly string      _configHeader;
    private readonly GatherBuddy _plugin;

    private FishManager FishManager
        => _plugin.Gatherer!.FishManager;

    private WeatherManager WeatherManager
        => _plugin.Gatherer!.WeatherManager;

    private readonly Cache.Icons  _icons;
    private readonly Cache.Header _headerCache;

    private Cache.Alarms?  _alarmCache;
    private Cache.FishTab? _fishCache;
    private Cache.NodeTab? _nodeTabCache;
    private Cache.Weather? _weatherCache;

    public bool Visible;

    private static float   _horizontalSpace;
    private static float   _globalScale;
    private static float   _textHeight;
    private static float   _minXSize;
    private static Vector2 _itemSpacing  = Vector2.Zero;
    private static Vector2 _framePadding = Vector2.Zero;
    private static float   _textHeightOffset;
    private static Vector2 _iconSize;
    private static Vector2 _smallIconSize;
    private static Vector2 _fishIconSize;
    private static Vector2 _weatherIconSize;
    private        float   _alarmsSpacing;

    public Interface(GatherBuddy plugin)
    {
        _plugin       = plugin;
        _configHeader = GatherBuddy.Version.Length > 0 ? $"{PluginName} v{GatherBuddy.Version}###GatherBuddyMain" : PluginName;
        _headerCache.Setup();

        var weatherSheet = Dalamud.GameData.GetExcelSheet<Weather>()!;
        _icons = Service<Cache.Icons>.Set((int)weatherSheet.RowCount + FishManager.FishByUptime.Count + FishManager.Bait.Count)!;

        if (GatherBuddy.Config.ShowFishFromPatch < Cache.FishTab.PatchSelector.Length)
            return;

        GatherBuddy.Config.ShowFishFromPatch = 0;
        GatherBuddy.Config.Save();
    }

    public void Dispose()
    {
        Service<Cache.Icons>.Dispose();
    }

    public void Draw()
    {
        if (!Visible)
            return;

        // Initialize style variables.
        _globalScale      = ImGui.GetIO().FontGlobalScale;
        _horizontalSpace  = DefaultHorizontalSpace * _globalScale;
        _minXSize         = 450f * _globalScale;
        _textHeight       = ImGui.GetTextLineHeightWithSpacing();
        _itemSpacing      = ImGui.GetStyle().ItemSpacing;
        _framePadding     = ImGui.GetStyle().FramePadding;
        _iconSize         = ImGuiHelpers.ScaledVector2(40, 40);
        _smallIconSize    = _iconSize / 2;
        _weatherIconSize  = ImGuiHelpers.ScaledVector2(30, 30);
        _fishIconSize     = new Vector2(_iconSize.X * ImGui.GetTextLineHeight() / _iconSize.Y, ImGui.GetTextLineHeight());
        _textHeightOffset = (_weatherIconSize.Y - ImGui.GetTextLineHeight()) / 2;

        ImGui.SetNextWindowSizeConstraints(
            new Vector2(_minXSize,     _textHeight * 17),
            new Vector2(_minXSize * 4, ImGui.GetIO().DisplaySize.Y * 15 / 16));

        using var raii = new ImGuiRaii();
        if (!raii.BeginWindow(_configHeader, ref Visible))
            return;

        var minute = TimeStamp.UtcNow.TotalEorzeaMinutes();
        var hour   = minute / RealTime.MinutesPerHour;

        DrawHeaderRow();
        DrawTimeRow(hour, minute);

        if (!raii.BeginTabBar("##Tabs", ImGuiTabBarFlags.NoTooltip | ImGuiTabBarFlags.Reorderable))
            return;

        var nodeTab = raii.BeginTabItem("Timed Nodes");
        ImGuiHelper.HoverTooltip("Shows timed nodes corresponding to the selection of the checkmarks below, sorted by next uptime.\n"
          + "Click on a node to do a /gather command for that node.");
        if (nodeTab)
        {
            _nodeTabCache ??= new Cache.NodeTab(_plugin.Gatherer!.Timeline);
            _nodeTabCache.Update(hour);
            DrawNodesTab();
            raii.End();
        }

        var fishTab = raii.BeginTabItem("Timed Fish");
        ImGuiHelper.HoverTooltip("Shows all fish for the fishing log and their next uptimes.\n"
          + "You can click the fish name or the fishing spot name to execute a /gatherfish command.\n"
          + "You can right-click a fish name to fix (or unfix) this fish at the top of the list.");
        if (fishTab)
        {
            _fishCache ??= new Cache.FishTab(WeatherManager, FishManager, _icons);
            _fishCache.UpdateFish();
            DrawFishTab();
            raii.End();
        }

        if (raii.BeginTabItem("Weather"))
        {
            _weatherCache ??= new Cache.Weather(WeatherManager);
            _weatherCache.Update(hour);
            DrawWeatherTab();
            raii.End();
        }

        var alertTab = raii.BeginTabItem("Alarms");
        ImGuiHelper.HoverTooltip("Setup alarms for specific timed gathering nodes.\n"
          + "You can use [/gather alarm] to directly gather the last triggered alarm.");
        if (alertTab)
        {
            _alarmCache ??= new Cache.Alarms(_plugin.Alarms!, GatherBuddy.Language);
            DrawAlarmsTab();
            raii.End();
        }

        if (raii.BeginTabItem("Settings"))
        {
            DrawSettingsTab();
            raii.End();
        }

        if (raii.BeginTabItem("Debug"))
        {
            if (GatherBuddy.GameData != null)
            {
                ImGuiTable.DrawTabbedTable($"Aetherytes ({GatherBuddy.GameData.Aetherytes.Count})", GatherBuddy.GameData.Aetherytes.Values, a =>
                {
                    ImGui.TableNextColumn();
                    ImGui.Text(a.Id.ToString());
                    ImGui.TableNextColumn();
                    ImGui.Text(a.Name);
                    ImGui.TableNextColumn();
                    ImGui.Text(a.Territory.Name);
                    ImGui.TableNextColumn();
                    ImGui.Text($"{a.XCoord}-{a.YCoord}");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{a.XStream}-{a.YStream}");
                }, ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit, "Id", "Name", "Territory", "Coords", "Aetherstream");

                ImGuiTable.DrawTabbedTable($"Territories ({GatherBuddy.GameData.WeatherTerritories.Length})", GatherBuddy.GameData.WeatherTerritories, t =>
                {
                    ImGui.TableNextColumn();
                    ImGui.Text(t.Id.ToString());
                    ImGui.TableNextColumn();
                    ImGui.Text(t.Name);
                    ImGui.TableNextColumn();
                    ImGui.Text(t.SizeFactor.ToString(CultureInfo.InvariantCulture));
                    ImGui.TableNextColumn();
                    ImGui.Text(t.WeatherRates.Rates.Length.ToString());
                }, ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit, "Id", "Name", "SizeFactor", "#Weathers");

                ImGuiTable.DrawTabbedTable($"Bait ({GatherBuddy.GameData.Bait.Count})", GatherBuddy.GameData.Bait.Values, b =>
                {
                    ImGui.TableNextColumn();
                    ImGui.Text(b.Id.ToString());
                    ImGui.TableNextColumn();
                    ImGui.Text(b.Name);
                }, ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit, "Id", "Name");

                ImGuiTable.DrawTabbedTable($"Gatherables ({GatherBuddy.GameData.Gatherables.Count})", GatherBuddy.GameData.Gatherables.Values.OrderBy((g, h) => g.ItemId.CompareTo(h.ItemId)), g =>
                {
                    ImGui.TableNextColumn();
                    ImGui.Text(g.ItemId.ToString());
                    ImGui.TableNextColumn();
                    ImGui.Text(g.GatheringId.ToString());
                    ImGui.TableNextColumn();
                    ImGui.Text(g.Name.English);
                    ImGui.TableNextColumn();
                    ImGui.Text(g.LevelString());
                    ImGui.TableNextColumn();
                    ImGui.Text(g.NodeList.Count.ToString());
                }, ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit, "ItemId", "GatheringId", "Name", "Level", "#Nodes");

                ImGuiTable.DrawTabbedTable($"Gathering Nodes ({GatherBuddy.GameData.GatheringNodes.Count})", GatherBuddy.GameData.GatheringNodes.Values, g =>
                {
                    ImGui.TableNextColumn();
                    ImGui.Text(g.BaseId.ToString());
                    ImGui.TableNextColumn();
                    ImGui.Text(g.Name);
                    ImGui.TableNextColumn();
                    ImGui.Text(g.GatheringType.ToString());
                    ImGui.TableNextColumn();
                    ImGui.Text(g.Level.ToString());
                    ImGui.TableNextColumn();
                    ImGui.Text(g.NodeType.ToString());
                    ImGui.TableNextColumn();
                    ImGui.Text($"{g.Territory.Name} ({g.Territory.Id})");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{g.XCoord}-{g.YCoord}");
                    ImGui.TableNextColumn();
                    ImGui.Text(g.ClosestAetheryte?.Name ?? "Unknown");
                    ImGui.TableNextColumn();
                    ImGui.Text(g.Folklore);
                    ImGui.TableNextColumn();
                    ImGui.Text(g.Times.PrintHours(true));
                    ImGui.TableNextColumn();
                    ImGui.Text(g.PrintItems());
                }, ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit, "Id", "Name", "Job", "Level", "Type", "Territory", "Coords", "Aetheryte", "Folklore", "Times", "Items");

                ImGuiTable.DrawTabbedTable($"Fish ({GatherBuddy.GameData.Fishes.Count})", GatherBuddy.GameData.Fishes.Values, f =>
                {
                    ImGui.TableNextColumn();
                    ImGui.Text(f.ItemId.ToString());
                    ImGui.TableNextColumn();
                    ImGui.Text($"{f.FishId}{(f.IsSpearFish ? " (sf)" : "")}");
                    ImGui.TableNextColumn();
                    ImGui.Text(f.Name.English);
                    ImGui.TableNextColumn();
                    ImGui.Text(f.FishRestrictions.ToString());
                    ImGui.TableNextColumn();
                    ImGui.Text(f.Folklore);
                    ImGui.TableNextColumn();
                    ImGui.Text(f.InLog.ToString());
                    ImGui.TableNextColumn();
                    ImGui.Text(f.IsBigFish.ToString());
                    ImGui.TableNextColumn();
                    ImGui.Text(string.Join('|', f.FishingSpots.Select(s => s.Name)));
                }, ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit, "ItemId", "FishId", "Name", "Restrictions", "Folklore", "InLog", "Big", "Fishing Spots");

                ImGuiTable.DrawTabbedTable($"Fishing Spots ({GatherBuddy.GameData.FishingSpots.Count})", GatherBuddy.GameData.FishingSpots.Values, f =>
                {
                    ImGui.TableNextColumn();
                    ImGui.Text($"{f.Id}{(f.Spearfishing ? " (sf)" : "")}");
                    ImGui.TableNextColumn();
                    ImGui.Text(f.Name);
                    ImGui.TableNextColumn();
                    ImGui.Text($"{f.Territory.Name} ({f.Territory.Id})");
                    ImGui.TableNextColumn();
                    ImGui.Text(f.ClosestAetheryte?.Name ?? "Unknown");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{f.IntegralXCoord/100f:00.00}-{f.IntegralYCoord/100f:00.00}");
                    ImGui.TableNextColumn();
                    ImGui.Text(string.Join('|', f.Items.Select(fish => fish.Name)));
                }, ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit, "Id", "Name", "Territory", "Aetheryte", "Coords", "Fishes");

                void PrintNode<T>(PatriciaTrie<T>.Node node)
                {
                    var name = node.TotalWord.ToString();
                    if (name.Length == 0)
                        name = "Root";
                    if (node.Children.Count == 0)
                        ImGui.Text(name);
                    else
                    {
                        if (!ImGui.TreeNodeEx(name))
                            return;

                        foreach (var child in node.Children)
                            PrintNode(child);
                        ImGui.TreePop();
                    }
                }

                if (ImGui.CollapsingHeader("GatheringTree"))
                {
                    ImGui.PushID("GatheringTree");
                    PrintNode(GatherBuddy.GameData.GatherablesTrie.Root); 
                    ImGui.PopID();
                }
                if (ImGui.CollapsingHeader("FishingTree"))
                {
                    ImGui.PushID("FishingTree");
                    PrintNode(GatherBuddy.GameData.FishTrie.Root);
                    ImGui.PopID();
                }
            }
            raii.End();
        }
    }
}
