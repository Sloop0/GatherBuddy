﻿using System;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Logging;
using GatherBuddy.Alarms;
using GatherBuddy.Config;
using GatherBuddy.Interfaces;
using GatherBuddy.Plugin;
using GatherBuddy.Time;
using ImGuiNET;
using ImGuiOtter;

namespace GatherBuddy.Gui;

public partial class Interface
{
    private static string CheckUnnamed(string name)
        => name.Length > 0 ? name : "<Unnamed>";

    private class AlarmCache
    {
        public sealed class TimedItemCombo : ClippedSelectableCombo<IGatherable>
        {
            public TimedItemCombo(string label)
                : base("##TimedItem", label, 200, GatherBuddy.UptimeManager.TimedGatherables, i => i.Name[GatherBuddy.Language])
            { }
        }

        public sealed class AlarmSelector : ItemSelector<AlarmGroup>
        {
            private readonly AlarmManager _manager;

            public AlarmSelector(AlarmManager manager)
                : base(manager.Alarms, Flags.All)
                => _manager = manager;

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
                _manager.DeleteGroup(idx);
                return true;
            }

            protected override bool OnAdd(string name)
            {
                _manager.AddGroup(name);
                return true;
            }

            protected override bool OnClipboardImport(string name, string data)
            {
                if (!AlarmGroup.Config.FromBase64(data, out var configGroup))
                    return false;

                var group = new AlarmGroup()
                {
                    Name        = name,
                    Description = configGroup.Description,
                    Enabled     = false,
                    Alarms = configGroup.Alarms.Select(a => Alarm.FromConfig(a, out var alarm) ? alarm : null)
                        .Where(a => a != null)
                        .Cast<Alarm>()
                        .ToList(),
                };

                if (group.Alarms.Count < configGroup.Alarms.Count())
                    PluginLog.Warning("Stuff happened"); // TODO

                _manager.AddGroup(group);
                return true;
            }

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

        public AlarmCache(AlarmManager manager)
            => Selector = new AlarmSelector(manager);

        public static readonly Sounds[] SoundIds = Enum.GetValues<Sounds>().Where(s => s != Sounds.Unknown).ToArray();
        public static readonly string SoundIdNames = string.Join("\0", SoundIds.Select(s => s == Sounds.None ? "None" : $"Sound {s.ToIdx()}"));

        public readonly AlarmSelector  Selector;
        public readonly TimedItemCombo ItemCombo = new(string.Empty);

        public bool EditGroupName; // TODO
        public bool EditGroupDesc;

        public string NewName         = string.Empty;
        public int    NewItemIdx      = 0;
        public bool   NewEnabled      = false;
        public bool   NewPrintMessage = false;
        public int    NewSoundIdx     = 0;
        public int    NewSecondOffset = 0;

        public Alarm CreateAlarm()
            => new(GatherBuddy.UptimeManager.TimedGatherables[NewItemIdx])
            {
                Enabled      = NewEnabled,
                SecondOffset = NewSecondOffset,
                PrintMessage = NewPrintMessage,
                Name         = NewName,
                SoundId      = SoundIds[NewSoundIdx],
            };
    }

    private readonly AlarmCache _alarmCache;

    private void DrawAlarmInfo(ref int alarmIdx, AlarmGroup group)
    {
        var       alarm   = group.Alarms[alarmIdx];
        using var id      = ImGuiRaii.PushId(alarmIdx);
        var       enabled = alarm.Enabled;

        ImGui.TableNextColumn();
        if (ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.Trash.ToIconString(), IconButtonSize, "Delete this Alarm...", false, true))
            _plugin.AlarmManager.DeleteAlarm(group, alarmIdx--);
        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(SetInputWidth);
        var name = alarm.Name;
        if (ImGui.InputTextWithHint("##name", CheckUnnamed(string.Empty), ref name, 64))
            _plugin.AlarmManager.ChangeAlarmName(group, alarmIdx, name);
        ImGuiUtil.HoverTooltip("Names are optional and can be used in the alarm message that is printed to chat.");

        ImGui.TableNextColumn();
        if (ImGui.Checkbox("##Enabled", ref enabled) && enabled != alarm.Enabled)
            _plugin.AlarmManager.ToggleAlarm(group, alarmIdx);
        ImGuiUtil.HoverTooltip("Enable this Alarm.");

        ImGui.TableNextColumn();
        if (_alarmCache.ItemCombo.Draw(alarm.Item.InternalLocationId - 1, out var newIdx))
            _plugin.AlarmManager.ChangeAlarmItem(group, alarmIdx, GatherBuddy.UptimeManager.TimedGatherables[newIdx]);

        ImGui.TableNextColumn();
        var secondOffset = alarm.SecondOffset;
        ImGui.SetNextItemWidth(SetInputWidth / 2);
        if (ImGui.DragInt("##Offset", ref secondOffset, 0.1f, 0, RealTime.SecondsPerDay))
            _plugin.AlarmManager.ChangeAlarmOffset(group, alarmIdx, Math.Clamp(secondOffset, 0, RealTime.SecondsPerDay));
        ImGuiUtil.HoverTooltip("Trigger this alarm this many seconds before the item in question is next available.");

        ImGui.TableNextColumn();
        var printMessage = alarm.PrintMessage;
        if (ImGui.Checkbox("##PrintMessage", ref printMessage))
            _plugin.AlarmManager.ChangeAlarmMessage(group, alarmIdx, printMessage);
        ImGuiUtil.HoverTooltip("Print a chat message when this alarm is triggered.");

        ImGui.TableNextColumn();
        var idx = alarm.SoundId.ToIdx();
        ImGui.SetNextItemWidth(85 * ImGuiHelpers.GlobalScale);
        if (ImGui.Combo("##Sound", ref idx, AlarmCache.SoundIdNames))
            _plugin.AlarmManager.ChangeAlarmSound(group, alarmIdx, AlarmCache.SoundIds[idx]);
        ImGuiUtil.HoverTooltip("Play this sound effect when this alarm is triggered.");

        ImGui.TableNextColumn();
        var (_, time) = GatherBuddy.UptimeManager.BestLocation(alarm.Item);
        var now  = GatherBuddy.Time.ServerTime.AddSeconds(alarm.SecondOffset);
        var size = Vector2.UnitX * 150;
        if (time.Start > now)
            ImGuiUtil.DrawTextButton(TimeInterval.DurationString(time.Start, now, false), size, ColorId.WarningBg.Value());
        else
            ImGuiUtil.DrawTextButton("Currently Triggered", size, ColorId.ChangedLocationBg.Value());
    }

    private void DrawAlarmInfo(AlarmGroup group, int idx)
    {
        if (!ImGui.BeginChild("##alarmInfo", -Vector2.One, false, ImGuiWindowFlags.HorizontalScrollbar))
        {
            ImGui.EndChild();
            return;
        }

        using var end = ImGuiRaii.DeferredEnd(ImGui.EndChild);
        ImGui.Text(group.Name);
        ImGui.Text(group.Description);
        var enabled = group.Enabled;
        if (ImGui.Checkbox("Enabled", ref enabled) && enabled != group.Enabled)
            _plugin.AlarmManager.ToggleGroup(idx);

        ImGui.NewLine();
        var width = SetInputWidth * 2.5f + ImGui.GetFrameHeight() * 3 + (85 + 150) * ImGuiHelpers.GlobalScale + ItemSpacing.X * 8;
        if (ImGui.BeginTable("##alarms", 8, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.NoKeepColumnsVisible, Vector2.UnitX * width))
        {
            end.Push(ImGui.EndTable);
            for (var i = 0; i < group.Alarms.Count; ++i)
                DrawAlarmInfo(ref i, group);

            using var id = ImGuiRaii.PushId(-1);
            ImGui.TableNextColumn();
            if (ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.Plus.ToIconString(), IconButtonSize, "Add new Alarm...", false, true))
                _plugin.AlarmManager.AddAlarm(group, _alarmCache.CreateAlarm());
            ImGui.TableNextColumn();
            ImGui.SetNextItemWidth(SetInputWidth);
            ImGui.InputTextWithHint("##name", CheckUnnamed(string.Empty), ref _alarmCache.NewName, 64);
            ImGui.TableNextColumn();
            ImGui.Checkbox("##enabled", ref _alarmCache.NewEnabled);
            ImGui.TableNextColumn();
            if (_alarmCache.ItemCombo.Draw(_alarmCache.NewItemIdx, out var tmp))
                _alarmCache.NewItemIdx = tmp;
            ImGui.TableNextColumn();
            ImGui.SetNextItemWidth(SetInputWidth / 2);
            if (ImGui.DragInt("##Offset", ref _alarmCache.NewSecondOffset, 0.1f, 0, RealTime.SecondsPerDay))
                _alarmCache.NewSecondOffset = Math.Clamp(_alarmCache.NewSecondOffset, 0, RealTime.SecondsPerDay);
            ImGui.TableNextColumn();
            ImGui.Checkbox("##print", ref _alarmCache.NewPrintMessage);
            ImGui.TableNextColumn();
            ImGui.SetNextItemWidth(85 * ImGuiHelpers.GlobalScale);
            ImGui.Combo("##Sound", ref _alarmCache.NewSoundIdx, AlarmCache.SoundIdNames);
        }
    }

    private void DrawAlarmGroupHeaderLine()
    {
        if (ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.Copy.ToIconString(), IconButtonSize, "Copy current Alarm Group to clipboard.",
                _alarmCache.Selector.Current == null, true))
        {
            var group = _alarmCache.Selector.Current!;
            try
            {
                var s = new AlarmGroup.Config(group).ToBase64();
                ImGui.SetClipboardText(s);
                Functions.Print($"Alarm Group {group.Name} saved to Clipboard.");
            }
            catch (Exception e)
            {
                var error = $"Could not write Alarm Group {group.Name} to Clipboard";
                PluginLog.Error($"{error}:\n{e}");
                Functions.PrintError($"{error}.");
            }
        }

        ImGui.SameLine();

        ImGuiComponents.HelpMarker("Use /gather alarm to gather the last item alarm triggered.\n"
          + "Use /gatherfish alarm to gather the last fish alarm triggered.");
    }

    private void DrawAlarmTab()
    {
        using var id = ImGuiRaii.PushId("Alarms");
        if (!ImGui.BeginTabItem("Alarms"))
            return;

        using var end = ImGuiRaii.DeferredEnd(ImGui.EndTabItem);

        _alarmCache.Selector.Draw(SelectorWidth);
        ImGui.SameLine();

        ItemDetailsWindow.Draw("Alarm Group Details", DrawAlarmGroupHeaderLine, () =>
        {
            if (_alarmCache.Selector.Current != null)
                DrawAlarmInfo(_alarmCache.Selector.Current, _alarmCache.Selector.CurrentIdx);
        });
    }
}
