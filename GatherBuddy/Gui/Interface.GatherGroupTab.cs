using System;
using GatherBuddy.Interfaces;
using GatherBuddy.Time;
using ImGuiNET;
using ImGuiOtter;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Logging;
using GatherBuddy.Config;
using GatherBuddy.GatherGroup;
using GatherBuddy.Plugin;

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

            Functions.Print($"Dropped {data?.ToString() ?? "NULL"} onto {Items[idx].Name} ({idx}).");
        }
    }

    private class GatherGroupCache
    {
        public static readonly IGatherable[] AllGatherables = GatherBuddy
            .GameData
            .Gatherables.Values
            .Concat(GatherBuddy.GameData.Fishes.Values.Cast<IGatherable>())
            .Where(g => g.Locations.Any())
            .OrderBy(g => g.Name[GatherBuddy.Language])
            .ToArray();

        public readonly ClippedSelectableCombo<IGatherable> GatherableSelector =
            new("AllGatherables", string.Empty, 250, AllGatherables, g => g.Name[GatherBuddy.Language]);

        public readonly Selector Selector;

        public bool NameEdit          = false;
        public bool DescriptionEdit   = false;
        public int  AnnotationEditIdx = -1;

        public readonly string DefaultGroupTooltip;
        public          int    NewItemIdx = 0;

        public static short[] UpdateItemPerMinute(TimedGroup group)
        {
            var times = new short[group.Nodes.Count + 1];
            for (var i = 0; i < RealTime.MinutesPerDay; ++i)
            {
                var node = group.CurrentNode((uint)i);
                if (node == null)
                {
                    times[0]++;
                }
                else
                {
                    var idx = group.Nodes.IndexOf(node);
                    times[idx + 1]++;
                }
            }

            return times;
        }

        public GatherGroupCache(Manager gatherGroupManager)
        {
            Selector = new Selector(gatherGroupManager);
            DefaultGroupTooltip =
                "Restore the gather groups provided by default if they have been deleted or changed in any way.\n"
              + "Hold Control to apply. Default Groups are:\n\t- "
              + $"{string.Join("\n\t- ", GroupData.DefaultGroups.Select(g => g.Name))}";
        }
    }

    private readonly GatherGroupCache _gatherGroupCache;

    private void DrawTimeInput(string label, float width, int value, Action<int> setter)
    {
        var       hour   = value / RealTime.MinutesPerHour;
        var       minute = value % RealTime.MinutesPerHour;
        using var group  = ImGuiRaii.NewGroup();
        using var id     = ImGuiRaii.PushId(label);
        ImGui.SetNextItemWidth(width);
        using var style  = ImGuiRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.One * 2 * ImGuiHelpers.GlobalScale);
        var       change = ImGui.DragInt("##hour", ref hour, 0.05f, 0, RealTime.HoursPerDay - 1, "%02d", ImGuiSliderFlags.AlwaysClamp);
        ImGui.SameLine();
        ImGui.Text(":");
        ImGui.SameLine();
        style.Pop();
        ImGui.SetNextItemWidth(width);
        change |= ImGui.DragInt("##minute", ref minute, 0.2f, 0, RealTime.MinutesPerHour - 1, "%02d", ImGuiSliderFlags.AlwaysClamp);

        if (change)
        {
            var newValue = Math.Clamp(hour * RealTime.MinutesPerHour + minute, 0, RealTime.MinutesPerDay - 1);
            if (newValue != value)
                setter(newValue);
        }
    }

    private void DrawTimeInput(int fromValue, int toValue, Action<int, int> setter)
    {
        var       width = 20 * ImGuiHelpers.GlobalScale;
        using var group = ImGuiRaii.NewGroup();

        ImGui.Text(" from ");
        ImGui.SameLine();
        DrawTimeInput("##from", width, fromValue, v => setter(v, toValue));
        ImGui.SameLine();
        ImGui.Text(" to ");
        ImGui.SameLine();
        DrawTimeInput("##to", width, toValue, v => setter(fromValue, v));
        ImGui.SameLine();
        ImGui.Text(" Eorzea Time");
    }

    private void DrawGatherGroupNode(TimedGroup group, ref int idx, int minutes)
    {
        var       node           = group.Nodes[idx];
        using var id             = ImGuiRaii.PushId(idx);
        var       i              = idx;
        var       annotationEdit = _gatherGroupCache.AnnotationEditIdx;
        ImGui.TableNextColumn();
        if (ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.Trash.ToIconString(), IconButtonSize, "Delete this item.", false, true))
            if (_plugin.GatherGroupManager.ChangeGroupNode(group, idx, null, null, null, null, true))
            {
                idx = idx == 0 ? 0 : -1;
                _plugin.GatherGroupManager.Save();
            }

        ImGui.TableNextColumn();
        if (_gatherGroupCache.GatherableSelector.Draw(node.Item.Name[GatherBuddy.Language], out var newIdx))
            if (_plugin.GatherGroupManager.ChangeGroupNode(@group, idx, GatherGroupCache.AllGatherables[newIdx], null, null, null, false))
                _plugin.GatherGroupManager.Save();

        _gatherGroupCache.Selector.CreateDropSource(node.Item.Name[GatherBuddy.Language]);

        ImGui.TableNextColumn();
        if (ImGui.GetContentRegionAvail().X >= 400 * ImGuiHelpers.GlobalScale)
            ImGui.SameLine();
        DrawTimeInput(node.EorzeaStartMinute, node.EorzeaEndMinute, (from, to) =>
        {
            if (_plugin.GatherGroupManager.ChangeGroupNode(group, i, null, from, to, null, false))
                _plugin.GatherGroupManager.Save();
        });
        ImGui.TableNextColumn();
        var length = node.Length();
        ImGuiUtil.DrawTextButton($"{length} minutes", Vector2.Zero,
            minutes < length ? ColorId.WarningBg.Value() : ImGui.GetColorU32(ImGuiCol.FrameBg));
        if (minutes < length)
            HoverTooltip($"{length - minutes} minutes are overwritten by overlap with earlier items.");


        ImGui.TableNextColumn();
        var annotation = node.Annotation;
        if (_gatherGroupCache.AnnotationEditIdx != idx)
        {
            ImGuiComponents.HelpMarker(annotation.Length > 0 ? annotation : "No annotation. Right-click to edit.");
            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            {
                _gatherGroupCache.AnnotationEditIdx = idx;
                ImGui.SetKeyboardFocusHere();
                ImGui.SetItemDefaultFocus();
                ImGui.SameLine();
                ImGui.SetNextItemWidth(0);
                ImGui.InputTextWithHint("##annotation", "Annotation...", ref annotation, 256, ImGuiInputTextFlags.EnterReturnsTrue);
            }
        }
        else
        {
            ImGui.SetNextItemWidth(400 * ImGuiHelpers.GlobalScale);
            if (ImGui.InputTextWithHint("##annotation", "Annotation...", ref annotation, 256, ImGuiInputTextFlags.EnterReturnsTrue)
             && _plugin.GatherGroupManager.ChangeGroupNode(group, idx, null, null, null, annotation, false))
                _plugin.GatherGroupManager.Save();
            if (annotationEdit == _gatherGroupCache.AnnotationEditIdx && !ImGui.IsItemActive())
                _gatherGroupCache.AnnotationEditIdx = -1;
        }
    }

    private void DrawMissingTimesHint(bool missingTimes)
    {
        if (missingTimes)
            ImGuiUtil.DrawTextButton("Not all minutes have a corresponding item.", new Vector2(-ImGui.GetStyle().WindowPadding.X, 0),
                ColorId.WarningBg.Value());
    }

    private void DrawGatherGroupNodeTable(TimedGroup group)
    {
        var times = GatherGroupCache.UpdateItemPerMinute(group);
        DrawMissingTimesHint(times[0] > 0);

        if (!ImGui.BeginTable("##nodes", 5, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.ScrollX))
            return;

        using var end = ImGuiRaii.DeferredEnd(ImGui.EndTable);

        for (var i = 0; i < group.Nodes.Count; ++i)
            DrawGatherGroupNode(group, ref i, times[i + 1]);

        var idx = _gatherGroupCache.NewItemIdx;
        ImGui.TableNextColumn();
        if (ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.Plus.ToIconString(), IconButtonSize, "Add new item...", false, true)
         && _plugin.GatherGroupManager.ChangeGroupNode(group, group.Nodes.Count, GatherGroupCache.AllGatherables[idx], null, null, null, false))
            _plugin.GatherGroupManager.Save();
        ImGui.TableNextColumn();
        if (_gatherGroupCache.GatherableSelector.Draw(GatherGroupCache.AllGatherables[idx].Name[GatherBuddy.Language], out idx))
            _gatherGroupCache.NewItemIdx = idx;
        ImGui.TableNextColumn();
        ImGui.TableNextColumn();
    }


    private void DrawNameField(TimedGroup group)
    {
        var       edit  = _gatherGroupCache.NameEdit;
        using var style = ImGuiRaii.PushStyle(ImGuiStyleVar.ItemSpacing, ItemSpacing / 2);
        using var id    = ImGuiRaii.PushId(0);
        if (ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.Edit.ToIconString(), IconButtonSize, "Rename",
                _gatherGroupCache.NameEdit, true))
            _gatherGroupCache.NameEdit = true;
        ImGui.SameLine();
        style.Pop();
        if (!_gatherGroupCache.NameEdit)
        {
            ImGuiUtil.DrawTextButton(group.Name, Vector2.Zero, ImGui.GetColorU32(ImGuiCol.FrameBg));
            return;
        }

        var newName = group.Name;
        ImGui.SetNextItemWidth(SetInputWidth);
        if (edit != _gatherGroupCache.NameEdit)
        {
            ImGui.SetKeyboardFocusHere();
            ImGui.SetItemDefaultFocus();
        }

        if (ImGui.InputText("##rename", ref newName, 64, ImGuiInputTextFlags.EnterReturnsTrue))
            if (_plugin.GatherGroupManager.RenameGroup(@group, newName))
                _plugin.GatherGroupManager.Save();
        if (edit == _gatherGroupCache.NameEdit && !ImGui.IsItemActive())
            _gatherGroupCache.NameEdit = false;

        if (newName != group.Name)
        {
            if (newName.Length == 0)
            {
                ImGui.SameLine();
                ImGuiUtil.DrawTextButton("Name can not be empty.", Vector2.Zero, ColorId.WarningBg.Value());
            }
            else if (_plugin.GatherGroupManager.Groups.ContainsKey(newName.ToLowerInvariant().Trim()))
            {
                ImGui.SameLine();
                ImGuiUtil.DrawTextButton("Name is already in use.", Vector2.Zero, ColorId.WarningBg.Value());
            }
        }
    }

    private void DrawDescField(TimedGroup group)
    {
        var       edit  = _gatherGroupCache.DescriptionEdit;
        using var style = ImGuiRaii.PushStyle(ImGuiStyleVar.ItemSpacing, ItemSpacing / 2);
        using var id    = ImGuiRaii.PushId(1);
        if (ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.Edit.ToIconString(), IconButtonSize, "Change Description",
                _gatherGroupCache.DescriptionEdit, true))
            _gatherGroupCache.DescriptionEdit = true;
        ImGui.SameLine();
        style.Pop();
        if (!_gatherGroupCache.DescriptionEdit)
        {
            ImGuiUtil.DrawTextButton(group.Description, Vector2.Zero, ImGui.GetColorU32(ImGuiCol.FrameBg));
            return;
        }

        var newDesc = group.Description;
        ImGui.SetNextItemWidth(2 * SetInputWidth * ImGuiHelpers.GlobalScale);
        if (edit != _gatherGroupCache.DescriptionEdit)
        {
            ImGui.SetKeyboardFocusHere();
            ImGui.SetItemDefaultFocus();
        }

        if (ImGui.InputText("##description", ref newDesc, 128, ImGuiInputTextFlags.EnterReturnsTrue))
            if (_plugin.GatherGroupManager.ChangeDescription(group, newDesc))
                _plugin.GatherGroupManager.Save();
        if (edit == _gatherGroupCache.DescriptionEdit && !ImGui.IsItemActive())
            _gatherGroupCache.DescriptionEdit = false;
    }

    private void DrawGatherGroup(TimedGroup group)
    {
        using var id = ImGuiRaii.PushId(group.Name);

        DrawNameField(group);
        DrawDescField(group);
        DrawGatherGroupNodeTable(group);
    }

    private void DrawHeaderLine()
    {
        if (ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.Copy.ToIconString(), IconButtonSize, "Copy current Gather Group to clipboard.",
                _gatherGroupCache.Selector.Current == null, true))
        {
            var group = _gatherGroupCache.Selector.Current!;
            try
            {
                var s = group.ToConfig().ToBase64();
                ImGui.SetClipboardText(s);
                Functions.Print($"Gather Group {group.Name} saved to Clipboard.");
            }
            catch (Exception e)
            {
                var error = $"Could not write Gather Group {group.Name} to Clipboard";
                PluginLog.Error($"{error}:\n{e}");
                Functions.PrintError($"{error}.");
            }
        }

        var       holdingCtrl = ImGui.GetIO().KeyCtrl;
        using var color       = ImGuiRaii.PushColor(ImGuiCol.ButtonHovered, 0x8000A000, holdingCtrl);
        if (ImGui.Button("Restore Default Groups") && holdingCtrl && _plugin.GatherGroupManager.SetDefaults(true))
        {
            _gatherGroupCache.Selector.TryRestoreCurrent();
            _plugin.GatherGroupManager.Save();
        }

        color.Pop();
        ImGuiUtil.HoverTooltip(_gatherGroupCache.DefaultGroupTooltip);

        ImGui.SameLine();

        ImGuiComponents.HelpMarker("Use /gathergroup [name] [optional:minute offset] to call a group.\n"
          + "This will /gather the item that is up currently (or [minute offset] eorzea minutes in the future).\n"
          + "If times for multiple items overlap, the first item from top to bottom will be gathered.");
    }

    private void DrawGatherGroupTab()
    {
        using var id = ImGuiRaii.PushId("Groups");
        if (!ImGui.BeginTabItem("Groups"))
            return;

        using var end = ImGuiRaii.DeferredEnd(ImGui.EndTabItem);

        _gatherGroupCache.Selector.Draw(SelectorWidth * ImGuiHelpers.GlobalScale);
        ImGui.SameLine();

        ItemDetailsWindow.Draw("Group Details", DrawHeaderLine, () =>
        {
            if (_gatherGroupCache.Selector.Current != null)
                DrawGatherGroup(_gatherGroupCache.Selector.Current);
        });
    }
}
