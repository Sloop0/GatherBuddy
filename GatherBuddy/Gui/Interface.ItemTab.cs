﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using Dalamud.Game.Gui.PartyFinder.Types;
using Dalamud.Interface;
using GatherBuddy.Config;
using GatherBuddy.Enums;
using GatherBuddy.Interfaces;
using GatherBuddy.Time;
using ImGuiNET;
using ImGuiOtter;
using ImGuiOtter.Table;

namespace GatherBuddy.Gui;

public partial class Interface
{
    private sealed class ItemTable : Table<ExtendedGatherable>, IDisposable
    {
        private static float _nameColumnWidth             = 0;
        private static float _nextUptimeColumnWidth       = 0;
        private static float _closestAetheryteColumnWidth = 0;
        private static float _levelColumnWidth            = 0;
        private static float _jobColumnWidth              = 0;
        private static float _typeColumnWidth             = 0;
        private static float _expansionColumnWidth        = 0;
        private static float _folkloreColumnWidth         = 0;
        private static float _uptimeColumnWidth           = 0;
        private static float _bestNodeColumnWidth         = 0;
        private static float _bestZoneColumnWidth         = 0;
        private static float _itemIdColumnWidth           = 0;
        private static float _gatheringIdColumnWidth      = 0;

        protected override void PreDraw()
        {
            if (_nameColumnWidth == 0)
            {
                _nameColumnWidth = (Items.Max(i => TextWidth(i.Data.Name[GatherBuddy.Language])) + ItemSpacing.X + LineIconSize.X) / Scale;
                _nextUptimeColumnWidth = Math.Max(TextWidth("99:99 Minutes") / Scale,
                    TextWidth(_nextUptimeColumn.Label) / Scale + Table.ArrowWidth);
                _closestAetheryteColumnWidth = GatherBuddy.GameData.Aetherytes.Values.Max(a => TextWidth(a.Name)) / Scale;
                _levelColumnWidth = Math.Max(TextWidth("99*****") / Scale,
                    TextWidth(_levelColumn.Label) / Scale + Table.ArrowWidth);
                _jobColumnWidth = Math.Max(TextWidth(_jobColumn.Label) / Scale + Table.ArrowWidth,
                    Enum.GetNames<GatheringType>().Where(s => s != "Spearfishing").Max(TextWidth) / Scale);
                _typeColumnWidth = Math.Max(TextWidth(_typeColumn.Label) / Scale + Table.ArrowWidth,
                    Enum.GetNames<NodeType>().Max(TextWidth) / Scale);
                _expansionColumnWidth   = TextWidth(_expansionColumn.Label) / Scale + Table.ArrowWidth;
                _folkloreColumnWidth    = Items.Max(i => TextWidth(i.Folklore)) / Scale;
                _uptimeColumnWidth      = Items.Max(i => TextWidth(i.Uptimes)) / Scale;
                _bestNodeColumnWidth    = GatherBuddy.GameData.GatheringNodes.Values.Max(a => TextWidth(a.Name)) / Scale;
                _bestZoneColumnWidth    = GatherBuddy.GameData.Territories.Values.Max(a => TextWidth(a.Name)) / Scale;
                _itemIdColumnWidth      = Math.Max(TextWidth("999999") / Scale, TextWidth(_itemIdColumn.Label) / Scale + Table.ArrowWidth);
                _gatheringIdColumnWidth = Math.Max(TextWidth("99999") / Scale,  TextWidth(_gatheringIdColumn.Label) / Scale + Table.ArrowWidth);
            }
        }

        private static readonly NameColumn        _nameColumn        = new() { Label = "Item Name..." };
        private static readonly NextUptimeColumn  _nextUptimeColumn  = new() { Label = "Next Uptime" };
        private static readonly AetheryteColumn   _aetheryteColumn   = new() { Label = "Aetheryte" };
        private static readonly LevelColumn       _levelColumn       = new() { Label = "Lvl..." };
        private static readonly JobColumn         _jobColumn         = new() { Label = "Gathering" };
        private static readonly TypeColumn        _typeColumn        = new() { Label = "Node Type" };
        private static readonly ExpansionColumn   _expansionColumn   = new() { Label = "Exp." };
        private static readonly FolkloreColumn    _folkloreColumn    = new() { Label = "Folklore" };
        private static readonly UptimesColumn     _uptimesColumn     = new() { Label = "Uptimes" };
        private static readonly BestNodeColumn    _bestNodeColumn    = new() { Label = "Best Node" };
        private static readonly BestZoneColumn    _bestZoneColumn    = new() { Label = "Best Zone" };
        private static readonly ItemIdColumn      _itemIdColumn      = new() { Label = "Item Id" };
        private static readonly GatheringIdColumn _gatheringIdColumn = new() { Label = "G. Id" };

        private class ItemFilterColumn : HeaderConfigFlags<ItemFilter, ExtendedGatherable>
        {
            private ItemFilter[] FlagValues = Array.Empty<ItemFilter>();
            private string[]     FlagNames  = Array.Empty<string>();

            protected void SetFlags(params ItemFilter[] flags)
            {
                FlagValues = flags;
                AllFlags   = FlagValues.Aggregate((f, g) => f | g);
            }

            protected void SetFlagsAndNames(params ItemFilter[] flags)
            {
                SetFlags(flags);
                SetNames(flags.Select(f => f.ToString()).ToArray());
            }

            protected void SetNames(params string[] names)
                => FlagNames = names;

            protected sealed override IReadOnlyList<ItemFilter> Values
                => FlagValues;

            protected sealed override string[] Names
                => FlagNames;

            public sealed override ItemFilter FilterValue
                => GatherBuddy.Config.ShowItems;

            protected sealed override void SetValue(ItemFilter f, bool v)
            {
                var tmp = v ? FilterValue | f : FilterValue & ~f;
                if (tmp == FilterValue)
                    return;

                GatherBuddy.Config.ShowItems = tmp;
                GatherBuddy.Config.Save();
            }
        }

        private sealed class NameColumn : HeaderConfigString<ExtendedGatherable>
        {
            public NameColumn()
                => Flags |= ImGuiTableColumnFlags.NoHide | ImGuiTableColumnFlags.NoReorder;

            public override string ToName(ExtendedGatherable item)
                => item.Data.Name[GatherBuddy.Language];

            public override float Width
                => _nameColumnWidth * ImGuiHelpers.GlobalScale;

            public override void DrawColumn(ExtendedGatherable item)
            {
                using var style = ImGuiRaii.PushStyle(ImGuiStyleVar.ItemSpacing, ItemSpacing / 2);
                ImGuiUtil.HoverIcon(item.Icon, LineIconSize);
                ImGui.SameLine();
                if (ImGui.Selectable(item.Data.Name[GatherBuddy.Language]))
                    _plugin.Executor.GatherItem(item.Data);
            }
        }

        private sealed class NextUptimeColumn : ItemFilterColumn
        {
            public override float Width
                => _nextUptimeColumnWidth * ImGuiHelpers.GlobalScale;

            public NextUptimeColumn()
            {
                Flags |= ImGuiTableColumnFlags.DefaultSort;
                SetFlags(ItemFilter.Available, ItemFilter.Unavailable);
                SetNames("Currently Available", "Currently Unavailable");
            }

            public override void DrawColumn(ExtendedGatherable item)
                => DrawTimeInterval(item.Uptime.Item2);

            public override int Compare(ExtendedGatherable lhs, ExtendedGatherable rhs)
                => lhs.Uptime.Item2.Compare(rhs.Uptime.Item2);

            public override bool FilterFunc(ExtendedGatherable item)
            {
                var (_, uptime) = item.Uptime;
                return FilterValue.HasFlag(uptime.InRange(GatherBuddy.Time.ServerTime)
                    ? ItemFilter.Available
                    : ItemFilter.Unavailable);
            }
        }

        private sealed class AetheryteColumn : HeaderConfigString<ExtendedGatherable>
        {
            public override string ToName(ExtendedGatherable item)
                => item.Uptime.Item1.ClosestAetheryte?.Name ?? "None";

            public override float Width
                => _closestAetheryteColumnWidth * ImGuiHelpers.GlobalScale;

            public override void DrawColumn(ExtendedGatherable item)
            {
                var aetheryte = item.Uptime.Item1.ClosestAetheryte;
                if (aetheryte == null)
                {
                    ImGui.Text("None");
                    return;
                }

                if (ImGui.Selectable(aetheryte.Name))
                    Executor.TeleportToAetheryte(aetheryte);
                HoverTooltip(item.Aetherytes);
            }

            public override bool FilterFunc(ExtendedGatherable item)
            {
                var name = item.Aetherytes;
                if (FilterValue.Length == 0)
                    return true;

                return FilterRegex?.IsMatch(name) ?? name.Contains(FilterValue, StringComparison.InvariantCultureIgnoreCase);
            }
        }

        private sealed class LevelColumn : HeaderConfigString<ExtendedGatherable>
        {
            public override string ToName(ExtendedGatherable item)
                => item.Level;

            public override float Width
                => _levelColumnWidth * ImGuiHelpers.GlobalScale;

            public override int Compare(ExtendedGatherable lhs, ExtendedGatherable rhs)
            {
                var diff = lhs.Data.Level - rhs.Data.Level;
                if (diff != 0)
                    return diff;

                return lhs.Data.Stars - rhs.Data.Stars;
            }
        }

        private sealed class JobColumn : ItemFilterColumn
        {
            public override float Width
                => _jobColumnWidth * ImGuiHelpers.GlobalScale;

            public JobColumn()
                => SetFlagsAndNames(ItemFilter.Mining, ItemFilter.Quarrying, ItemFilter.Logging, ItemFilter.Harvesting);

            public override void DrawColumn(ExtendedGatherable item)
                => ImGui.Text(item.Data.GatheringType.ToString());

            public override int Compare(ExtendedGatherable lhs, ExtendedGatherable rhs)
                => lhs.Data.GatheringType.CompareTo(rhs.Data.GatheringType);

            public override bool FilterFunc(ExtendedGatherable item)
            {
                return item.Data.GatheringType switch
                {
                    GatheringType.Mining     => FilterValue.HasFlag(ItemFilter.Mining),
                    GatheringType.Quarrying  => FilterValue.HasFlag(ItemFilter.Quarrying),
                    GatheringType.Logging    => FilterValue.HasFlag(ItemFilter.Logging),
                    GatheringType.Harvesting => FilterValue.HasFlag(ItemFilter.Harvesting),
                    GatheringType.Botanist   => (FilterValue & (ItemFilter.Logging | ItemFilter.Harvesting)) != 0,
                    GatheringType.Miner      => (FilterValue & (ItemFilter.Mining | ItemFilter.Quarrying)) != 0,
                    GatheringType.Multiple   => (FilterValue & AllFlags) != 0,
                    _                        => false,
                };
            }
        }

        private sealed class TypeColumn : ItemFilterColumn
        {
            public override float Width
                => _typeColumnWidth * ImGuiHelpers.GlobalScale;

            public TypeColumn()
                => SetFlagsAndNames(ItemFilter.Regular, ItemFilter.Unspoiled, ItemFilter.Ephemeral, ItemFilter.Legendary);

            public override void DrawColumn(ExtendedGatherable item)
                => ImGui.Text(item.Data.NodeType.ToString());

            public override int Compare(ExtendedGatherable lhs, ExtendedGatherable rhs)
                => lhs.Data.NodeType.CompareTo(rhs.Data.NodeType);

            public override bool FilterFunc(ExtendedGatherable item)
            {
                return item.Data.NodeType switch
                {
                    NodeType.Regular   => FilterValue.HasFlag(ItemFilter.Regular),
                    NodeType.Unspoiled => FilterValue.HasFlag(ItemFilter.Unspoiled),
                    NodeType.Ephemeral => FilterValue.HasFlag(ItemFilter.Ephemeral),
                    NodeType.Legendary => FilterValue.HasFlag(ItemFilter.Legendary),
                    _                  => false,
                };
            }
        }

        private sealed class ExpansionColumn : ItemFilterColumn
        {
            public override float Width
                => _expansionColumnWidth * ImGuiHelpers.GlobalScale;

            public ExpansionColumn()
            {
                SetFlags(ItemFilter.ARealmReborn, ItemFilter.Heavensward, ItemFilter.Stormblood, ItemFilter.Shadowbringers,
                    ItemFilter.Endwalker);
                SetNames("A Realm Reborn", "Heavensward", "Stormblood", "Shadowbringers", "Endwalker");
            }

            public override void DrawColumn(ExtendedGatherable item)
                => ImGui.Text(item.Expansion);

            public override int Compare(ExtendedGatherable lhs, ExtendedGatherable rhs)
                => lhs.Data.ExpansionIdx.CompareTo(rhs.Data.ExpansionIdx);

            public override bool FilterFunc(ExtendedGatherable item)
            {
                return item.Data.ExpansionIdx switch
                {
                    0 => FilterValue.HasFlag(ItemFilter.ARealmReborn),
                    1 => FilterValue.HasFlag(ItemFilter.Heavensward),
                    2 => FilterValue.HasFlag(ItemFilter.Stormblood),
                    3 => FilterValue.HasFlag(ItemFilter.Shadowbringers),
                    4 => FilterValue.HasFlag(ItemFilter.Endwalker),
                    _ => false,
                };
            }
        }

        private sealed class FolkloreColumn : HeaderConfigString<ExtendedGatherable>
        {
            public override string ToName(ExtendedGatherable item)
                => item.Folklore;

            public override float Width
                => _folkloreColumnWidth * ImGuiHelpers.GlobalScale;
        }

        private sealed class UptimesColumn : HeaderConfigString<ExtendedGatherable>
        {
            public override string ToName(ExtendedGatherable item)
                => item.Uptimes;

            public override float Width
                => _uptimeColumnWidth * ImGuiHelpers.GlobalScale;
        }

        private sealed class BestNodeColumn : HeaderConfigString<ExtendedGatherable>
        {
            public override string ToName(ExtendedGatherable item)
                => item.Uptime.Item1.Name;

            public override float Width
                => _bestNodeColumnWidth * ImGuiHelpers.GlobalScale;

            public override void DrawColumn(ExtendedGatherable item)
            {
                if (ImGui.Selectable(ToName(item)))
                    _plugin.Executor.GatherLocation(item.Uptime.Item1);
                HoverTooltip(item.NodeNames);
            }

            public override bool FilterFunc(ExtendedGatherable item)
            {
                var name = item.NodeNames;
                if (FilterValue.Length == 0)
                    return true;

                return FilterRegex?.IsMatch(name) ?? name.Contains(FilterValue, StringComparison.InvariantCultureIgnoreCase);
            }
        }

        private sealed class BestZoneColumn : HeaderConfigString<ExtendedGatherable>
        {
            public override string ToName(ExtendedGatherable item)
                => item.Uptime.Item1.Territory.Name;

            public override float Width
                => _bestZoneColumnWidth * ImGuiHelpers.GlobalScale;

            public override void DrawColumn(ExtendedGatherable item)
            {
                if (ImGui.Selectable(ToName(item)))
                    Executor.TeleportToTerritory(item.Uptime.Item1.Territory);
                HoverTooltip(item.Territories);
            }

            public override bool FilterFunc(ExtendedGatherable item)
            {
                var name = item.Territories;
                if (FilterValue.Length == 0)
                    return true;

                return FilterRegex?.IsMatch(name) ?? name.Contains(FilterValue, StringComparison.InvariantCultureIgnoreCase);
            }
        }

        private sealed class ItemIdColumn : HeaderConfig<ExtendedGatherable>
        {
            public override float Width
                => _itemIdColumnWidth;

            public override int Compare(ExtendedGatherable lhs, ExtendedGatherable rhs)
                => lhs.Data.ItemId.CompareTo(rhs.Data.ItemId);

            public override void DrawColumn(ExtendedGatherable item)
                => ImGuiUtil.RightAlign($"{item.Data.ItemId}");
        }

        private sealed class GatheringIdColumn : HeaderConfig<ExtendedGatherable>
        {
            public override float Width
                => _gatheringIdColumnWidth;

            public override int Compare(ExtendedGatherable lhs, ExtendedGatherable rhs)
                => lhs.Data.GatheringId.CompareTo(rhs.Data.GatheringId);

            public override void DrawColumn(ExtendedGatherable item)
                => ImGuiUtil.RightAlign($"{item.Data.GatheringId}");
        }

        public ItemTable()
            : base("ItemTable",
                GatherBuddy.GameData.Gatherables.Values.Where(g => g.GatheringType != GatheringType.Unknown)
                    .Select(g => new ExtendedGatherable(g)).ToList(), TextHeight, _nameColumn, _nextUptimeColumn, _aetheryteColumn,
                _levelColumn, _jobColumn, _typeColumn, _expansionColumn, _folkloreColumn, _uptimesColumn, _bestNodeColumn, _bestZoneColumn,
                _itemIdColumn, _gatheringIdColumn)
        {
            Sortable                               =  true;
            GatherBuddy.UptimeManager.UptimeChange += OnUptimeChange;
            Flags                                  |= ImGuiTableFlags.Hideable | ImGuiTableFlags.Reorderable | ImGuiTableFlags.Resizable;
        }


        public void Dispose()
        {
            GatherBuddy.UptimeManager.UptimeChange -= OnUptimeChange;
        }

        private void OnUptimeChange(IGatherable item)
        {
            if (item.Type != ObjectType.Gatherable)
                return;

            FilterDirty = true;
        }
    }

    private readonly ItemTable _itemTable = new();

    private void DrawItemTab()
    {
        using var id = ImGuiRaii.PushId("Gatherables");
        if (!ImGui.BeginTabItem("Gatherables"))
            return;

        using var end = ImGuiRaii.DeferredEnd(ImGui.EndTabItem);
        _itemTable.Draw();
    }
}
