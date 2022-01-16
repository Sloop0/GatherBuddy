using System;
using GatherBuddy.Interfaces;
using GatherBuddy.Time;
using ImGuiNET;
using ImGuiOtter;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using GatherBuddy.GatherGroup;

namespace GatherBuddy.Gui;

public partial class Interface
{
    private sealed class Selector : ItemSelector<TimedGroup>
    {
        private readonly Manager _manager;

        public Selector(Manager manager)
            : base(manager.Groups.Values, Flags.All & ~Flags.Move)
            => _manager = manager;

        protected override bool Filtered(int idx)
            => Filter.Length != 0 && !Items[idx].Name.Contains(Filter, StringComparison.InvariantCultureIgnoreCase);

        protected override bool OnDraw(int idx)
        {
            using var id = ImGuiRaii.PushId(idx);
            return ImGui.Selectable(Items[idx].Name, idx == CurrentIdx);
        }

        protected override bool OnDelete(int idx)
        {
            if (Items.Count <= idx || idx < 0)
                return false;

            _manager.Groups.RemoveAt(idx);
            _manager.Save();
            return true;
        }

        protected override bool OnAdd(string name)
            => _manager.AddGroup(name, new TimedGroup(name));

        protected override bool OnClipboardImport(string name, string data)
        {
            if (!TimedGroup.Config.FromBase64(data, out var cfgGroup))
                return false;

            TimedGroup.FromConfig(cfgGroup, out var group);
            group.Name = name;
            return _manager.AddGroup(name, group);
        }

        protected override bool OnDuplicate(string name, int idx)
        {
            if (Items.Count <= idx || idx < 0)
                return false;

            var group = _manager.Groups.Values[idx].Clone(name);
            return _manager.AddGroup(name, group);
        }

        protected override void OnDrop(object? data, int idx)
        {
            if (Items.Count <= idx || idx < 0)
                return;

            Dalamud.Chat.Print($"Dropped {data?.ToString() ?? "NULL"} onto {Items[idx].Name} ({idx}).");
        }
    }

    private struct GatherGroupCache
    {
        public readonly ClippedSelectableCombo<IGatherable> AllGatherables = new("AllGatherables", string.Empty, 200, GatherBuddy
                .GameData
                .Gatherables.Values
                .Concat(GatherBuddy.GameData.Fishes.Values.Cast<IGatherable>())
                .OrderBy(g => g.Name[GatherBuddy.Language])
                .ToArray(),
            g => g.Name[GatherBuddy.Language]);

        private readonly IList<TimedGroup>                  _timedGroups;
        private readonly ClippedSelectableCombo<TimedGroup> _timedGroupsSelector;

        private int _currentSelection = 0;

        public readonly Selector Selector;

        public TimedGroup? Current { get; private set; }

        public GatherGroupCache(Manager gatherGroupManager)
        {
            Selector     = new Selector(gatherGroupManager);
            _timedGroups = gatherGroupManager.Groups.Values;
            _timedGroupsSelector = new ClippedSelectableCombo<TimedGroup>("TimedGroups", string.Empty, 250, _timedGroups,
                g => g.Name);
            Current = _timedGroups.Count > _currentSelection ? _timedGroups[_currentSelection] : null;
        }

        public void DrawTimedGroupSelector()
        {
            if (!ReferenceEquals(_timedGroups[_currentSelection], Current))
                _currentSelection = _timedGroups.IndexOf(Current);

            if (!_timedGroupsSelector.Draw(_currentSelection, out var newIdx))
                return;

            _currentSelection = newIdx;
            if (_currentSelection >= 0 && _currentSelection < _timedGroups.Count)
                Current = _timedGroups[_currentSelection];
            else
                Current = null;
        }
    }

    private GatherGroupCache _gatherGroupCache;

    private void DrawGatherGroupNode(TimedGroup group, int idx)
    {
        var       node = group.Nodes[idx];
        using var id   = ImGuiRaii.PushId(idx);
        if (_gatherGroupCache.AllGatherables.Draw(node.Item.Name[GatherBuddy.Language], out var newIdx))
            Dalamud.Chat.Print($"{newIdx}");
        ImGui.Selectable(node.Item.Name[GatherBuddy.Language]);
        _gatherGroupCache.Selector.CreateDropSource(node.Item.Name[GatherBuddy.Language]);
        ImGui.SameLine();
        ImGui.Text((node.EorzeaStartMinute / RealTime.MinutesPerHour).ToString());
        ImGui.SameLine();
        ImGui.Text((node.EorzeaStartMinute % RealTime.MinutesPerHour).ToString());
        ImGui.Text(node.Annotation);
        ImGui.SameLine();
        ImGui.Text((node.EorzeaEndMinute / RealTime.MinutesPerHour).ToString());
        ImGui.SameLine();
        ImGui.Text((node.EorzeaEndMinute % RealTime.MinutesPerHour).ToString());
        ImGui.SameLine();
    }

    private void DrawGatherGroup(TimedGroup group)
    {
        using var id   = ImGuiRaii.PushId(group.Name);
        var       name = group.Name;
        ImGui.SetNextItemWidth(250 * ImGuiHelpers.GlobalScale);
        if (ImGui.InputTextWithHint("##name", "Rename...", ref name, 32, ImGuiInputTextFlags.EnterReturnsTrue)
         && _plugin.GatherGroupManager.RenameGroup(group, name))
            _plugin.GatherGroupManager.Save();
        if (name != group.Name)
        {
            if (name.Length == 0)
            {
                ImGui.SameLine();
                using var color = ImGuiRaii.PushColor(ImGuiCol.Text, 0xFF0000FF);
                ImGui.Text("Name can not be empty.");
            }
            else if (_plugin.GatherGroupManager.Groups.ContainsKey(name.ToLowerInvariant().Trim()))
            {
                ImGui.SameLine();
                using var color = ImGuiRaii.PushColor(ImGuiCol.Text, 0xFF0000FF);
                ImGui.Text("Name is already in use.");
            }
        }


        var desc = group.Description;
        ImGui.SetNextItemWidth(250 * ImGuiHelpers.GlobalScale);
        if (ImGui.InputTextWithHint("##desc", "Optional Description...", ref desc, 128, ImGuiInputTextFlags.EnterReturnsTrue)
         && _plugin.GatherGroupManager.ChangeDescription(group, desc))
            _plugin.GatherGroupManager.Save();

        for (var nodeIdx = 0; nodeIdx < group.Nodes.Count; nodeIdx++)
            DrawGatherGroupNode(group, nodeIdx);
    }

    private void DrawGatherGroupTab()
    {
        using var id = ImGuiRaii.PushId("Groups");
        if (!ImGui.BeginTabItem("Groups"))
            return;

        using var end = ImGuiRaii.DeferredEnd(ImGui.EndTabItem);

        _gatherGroupCache.Selector.Draw(200 * ImGuiHelpers.GlobalScale);

        ImGui.SameLine();
        ImGui.BeginGroup();
        if (ImGui.Button("Restore Defaults"))
            _plugin.GatherGroupManager.SetDefaults(true);

        if (!ImGui.BeginChild("groupChild", -Vector2.One))
        {
            ImGui.EndChild();
            return;
        }

        end.Push(ImGui.EndChild);
        if (_gatherGroupCache.Selector.Current != null)
            DrawGatherGroup(_gatherGroupCache.Selector.Current);
        ImGui.EndGroup();
    }
}
