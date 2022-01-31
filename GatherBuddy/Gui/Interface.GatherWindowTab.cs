using System;
using System.Numerics;
using Dalamud.Interface;
using GatherBuddy.Config;
using GatherBuddy.GatherHelper;
using GatherBuddy.Plugin;
using ImGuiNET;
using ImGuiOtter;

namespace GatherBuddy.Gui;

public partial class Interface
{
    private class GatherWindowCache
    {
        public class GatherWindowSelector : ItemSelector<GatherWindowPreset>
        {
            public GatherWindowSelector()
                : base(_plugin.GatherWindowManager.Presets, Flags.All)
            { }

            protected override bool Filtered(int idx)
                => Filter.Length != 0 && !Items[idx].Name.Contains(Filter, StringComparison.InvariantCultureIgnoreCase);

            protected override bool OnDraw(int idx)
            {
                using var id    = ImGuiRaii.PushId(idx);
                using var color = ImGuiRaii.PushColor(ImGuiCol.Text, ColorId.DisabledText.Value(), !Items[idx].Enabled);
                return ImGui.Selectable(CheckUnnamed(Items[idx].Name), idx == CurrentIdx);
            }

            protected override bool OnDelete(int idx)
            {
                _plugin.GatherWindowManager.DeletePreset(idx);
                return true;
            }

            protected override bool OnAdd(string name)
            {
                _plugin.GatherWindowManager.AddPreset(new GatherWindowPreset()
                {
                    Name = name,
                });
                return true;
            }

            protected override bool OnClipboardImport(string name, string data)
            {
                if (!GatherWindowPreset.Config.FromBase64(data, out var cfg))
                    return false;

                GatherWindowPreset.FromConfig(cfg, out var preset);
                preset.Name = name;
                _plugin.GatherWindowManager.AddPreset(preset);
                return true;
            }

            protected override bool OnDuplicate(string name, int idx)
            {
                if (Items.Count <= idx || idx < 0)
                    return false;

                var preset = _plugin.GatherWindowManager.Presets[idx].Clone();
                preset.Name = name;
                _plugin.GatherWindowManager.AddPreset(preset);
                return true;
            }

            protected override void OnDrop(object? data, int idx)
            {
                if (Items.Count <= idx || idx < 0)
                    return;

                Functions.Print($"Dropped {data?.ToString() ?? "NULL"} onto {Items[idx].Name} ({idx}).");
            }
        }

        public readonly GatherWindowSelector Selector = new();

        public int NewGatherableIdx = 0;
    }

    private readonly GatherWindowCache _gatherWindowCache;

    private void DrawGatherWindowPresetHeaderLine()
    {
        //TODO
    }

    private void DrawGatherWindowPreset(GatherWindowPreset preset)
    {
        ImGuiUtil.DrawTextButton(CheckUnnamed(preset.Name), Vector2.Zero, ImGui.GetColorU32(ImGuiCol.FrameBg));
        ImGuiUtil.DrawTextButton(preset.Description,        Vector2.Zero, ImGui.GetColorU32(ImGuiCol.FrameBg));
        var tmp = preset.Enabled;
        if (ImGui.Checkbox("Enabled##preset", ref tmp) && tmp != preset.Enabled)
            _plugin.GatherWindowManager.TogglePreset(preset);
        if (!ImGui.BeginListBox("##gatherWindowList", -Vector2.One))
            return;

        using var end = ImGuiRaii.DeferredEnd(ImGui.EndListBox);
        for (var i = 0; i < preset.Items.Count; ++i)
        {
            using var id   = ImGuiRaii.PushId(i);
            var       item = preset.Items[i];
            if (ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.Trash.ToIconString(), IconButtonSize, "Delete this item from the preset...", false,
                    true))
                _plugin.GatherWindowManager.RemoveItem(preset, i--);

            ImGui.SameLine();
            if (_gatherGroupCache.GatherableSelector.Draw(item.Name[GatherBuddy.Language], out var newIdx))
                _plugin.GatherWindowManager.ChangeItem(preset, GatherGroupCache.AllGatherables[newIdx], i);
        }

        if (ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.Plus.ToIconString(), IconButtonSize,
                "Add this item at the end of the preset, if it is not already included...",
                preset.Items.Contains(GatherGroupCache.AllGatherables[_gatherWindowCache.NewGatherableIdx]), true))
            _plugin.GatherWindowManager.AddItem(preset, GatherGroupCache.AllGatherables[_gatherWindowCache.NewGatherableIdx]);

        ImGui.SameLine();
        if (_gatherGroupCache.GatherableSelector.Draw(_gatherWindowCache.NewGatherableIdx, out var idx))
            _gatherWindowCache.NewGatherableIdx = idx;
    }

    private void DrawGatherWindowTab()
    {
        using var id  = ImGuiRaii.PushId("GatherWindow");
        var       tab = ImGui.BeginTabItem("Gather Window");

        ImGuiUtil.HoverTooltip(
            "Config window too big? Why can't you hold all this information?\n"
          + "Prepare a small window with only those items that actually interest you!");

        if (!tab)
            return;

        using var end = ImGuiRaii.DeferredEnd(ImGui.EndTabItem);

        _gatherWindowCache.Selector.Draw(SelectorWidth);
        ImGui.SameLine();

        ItemDetailsWindow.Draw("Preset Details", DrawGatherWindowPresetHeaderLine, () =>
        {
            if (_gatherWindowCache.Selector.Current != null)
                DrawGatherWindowPreset(_gatherWindowCache.Selector.Current);
        });
    }
}
