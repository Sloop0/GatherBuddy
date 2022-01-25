using System;
using System.Numerics;
using GatherBuddy.Alarms;
using GatherBuddy.Config;
using GatherBuddy.Plugin;
using ImGuiNET;
using ImGuiOtter;

namespace GatherBuddy.Gui;

public partial class Interface
{
    private sealed class AlarmSelector : ItemSelector<AlarmGroup>
    {
        private readonly AlarmManager _manager;

        public AlarmSelector(AlarmManager manager)
            : base(manager.Alarms, Flags.All & ~Flags.Import)
            => _manager = manager;

        protected override bool Filtered(int idx)
            => Filter.Length != 0 && !Items[idx].Name.Contains(Filter, StringComparison.InvariantCultureIgnoreCase);

        protected override bool OnDraw(int idx)
        {
            using var id    = ImGuiRaii.PushId(idx);
            using var color = ImGuiRaii.PushColor(ImGuiCol.Text, ColorId.DisabledText.Value(), !Items[idx].Enabled);
            return ImGui.Selectable(Items[idx].Name, idx == CurrentIdx);
        }

        protected override bool OnDelete(int idx)
        {
            _manager.DeleteGroup(idx);
            return true;
        }

        protected override bool OnAdd(string name)
        {
            _manager.AddGroup(name);
            return true;
        }

        protected override bool OnClipboardImport(string name, string data)
            //if (!TimedGroup.Config.FromBase64(data, out var cfgGroup))
            //    return false;
            //
            //TimedGroup.FromConfig(cfgGroup, out var group);
            //group.Name = name;
            //return _manager.AddGroup(name, group);
            => true;

        protected override bool OnDuplicate(string name, int idx)
        {
            var group = _manager.Alarms[idx].Clone();
            group.Name = name;
            _manager.AddGroup(group);
            return true;
        }

        protected override void OnDrop(object? data, int idx)
        {
            if (Items.Count <= idx || idx < 0)
                return;

            Functions.Print($"Dropped {data?.ToString() ?? "NULL"} onto {Items[idx].Name} ({idx}).");
        }

        protected override bool OnMove(int idx1, int idx2)
        {
            _manager.MoveGroup(idx1, idx2);
            return idx1 != idx2;
        }
    }

    private AlarmSelector? _alarmSelector;

    private void DrawAlarmInfo(AlarmGroup? group, int idx)
    {
        using var end = ImGuiRaii.NewGroup();
        ImGui.Button("Nope");
        if (!ImGui.BeginChild("##alarmInfo", -Vector2.One, true))
        {
            ImGui.EndChild();
            return;
        }

        end.Push(ImGui.EndChild);
        if (group != null)
        {
            ImGui.Text(group.Name);
            ImGui.Text(group.Description);
            var enabled = group.Enabled;
            if (ImGui.Checkbox("Enabled", ref enabled) && enabled != group.Enabled)
                _plugin.AlarmManager.ToggleGroup(idx);
        }
    }

    private void DrawAlarmTab()
    {
        using var id = ImGuiRaii.PushId("Alarms");
        if (!ImGui.BeginTabItem("Alarms"))
            return;

        _alarmSelector ??= new AlarmSelector(_plugin.AlarmManager);
        _alarmSelector.Draw(SelectorWidth);
        ImGui.SameLine();
        DrawAlarmInfo(_alarmSelector.Current, _alarmSelector.CurrentIdx);
        using var end = ImGuiRaii.DeferredEnd(ImGui.EndTabItem);
    }
}
