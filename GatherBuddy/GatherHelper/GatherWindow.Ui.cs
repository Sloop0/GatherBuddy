﻿using System.ComponentModel.Design;
using System.Numerics;
using Dalamud.Interface;
using GatherBuddy.Alarms;
using GatherBuddy.Config;
using GatherBuddy.Gui;
using GatherBuddy.Interfaces;
using GatherBuddy.Plugin;
using GatherBuddy.Time;
using ImGuiNET;
using ImGuiOtter;
using Lumina.Data.Parsing.Layer;

namespace GatherBuddy.GatherHelper;

public class GatherWindow
{
    private readonly GatherBuddy _plugin;

    public GatherWindow(GatherBuddy plugin)
        => _plugin = plugin;

    private static void DrawTime(ILocation? loc, TimeInterval time)
    {
        if (!GatherBuddy.Config.ShowGatherWindowTimers || !ImGui.TableNextColumn())
            return;
        if (time.Equals(TimeInterval.Always))
            return;

        if (time.Start > GatherBuddy.Time.ServerTime)
        {
            using var color = ImGuiRaii.PushColor(ImGuiCol.Text, ColorId.UpcomingItem.Value());
            ImGui.Text(TimeInterval.DurationString(time.Start, GatherBuddy.Time.ServerTime, false));
        }
        else
        {
            using var color = ImGuiRaii.PushColor(ImGuiCol.Text, ColorId.AvailableItem.Value());
            ImGui.Text(TimeInterval.DurationString(time.End, GatherBuddy.Time.ServerTime, false));
        }

        CreateTooltip(loc, time);
    }

    private static void CreateTooltip(ILocation? loc, TimeInterval time)
    {
        if (!ImGui.IsItemHovered())
            return;

        var s = string.Empty;
        if (loc == null)
            s += "Unknown Location\nUnknown Territory\nUnknown Aetheryte\n";
        else
            s += $"{loc.Name}\n{loc.Territory.Name}\n{loc.ClosestAetheryte?.Name ?? "No Aetheryte"}\n";

        if (time.Equals(TimeInterval.Always))
            s += "Always Up";
        else
            s += $"{time.Start}\n{time.End}\n{time.DurationString()}\n{TimeInterval.DurationString(time.Start > GatherBuddy.Time.ServerTime ? time.Start : time.End, GatherBuddy.Time.ServerTime, false)}";

        ImGui.SetTooltip(s);
    }

    private void DrawAlarm((Alarm, ILocation, TimeInterval)? alarmGroup)
    {
        if (alarmGroup == null)
            return;

        var (alarm, location, time) = alarmGroup.Value;
        if (time.End < GatherBuddy.Time.ServerTime)
            return;

        if (ImGui.TableNextColumn())
        {
            var name = alarm.Name.Length == 0
                ? $"Alarm: {alarm.Item.Name[GatherBuddy.Language]}"
                : $"{alarm.Name}: {alarm.Item.Name[GatherBuddy.Language]}";
            using var style = ImGuiRaii.PushStyle(ImGuiStyleVar.ItemSpacing, ImGui.GetStyle().ItemSpacing / 2);
            ImGuiUtil.HoverIcon(Icons.DefaultStorage[alarm.Item.ItemData.Icon], Vector2.One * ImGui.GetTextLineHeight());
            ImGui.SameLine();
            using var color = ImGuiRaii.PushColor(ImGuiCol.Text, ColorId.AvailableItem.Value());
            if (ImGui.Selectable(name, false))
                _plugin.Executor.GatherLocation(location);
            color.Pop();
            style.Pop();
            CreateTooltip(location, time);
        }

        DrawTime(location, time);
    }

    private void DrawItem(IGatherable item)
    {
        var (loc, time) = GatherBuddy.UptimeManager.BestLocation(item);
        if (ImGui.TableNextColumn())
        {
            using var style = ImGuiRaii.PushStyle(ImGuiStyleVar.ItemSpacing, ImGui.GetStyle().ItemSpacing / 2);
            ImGuiUtil.HoverIcon(Icons.DefaultStorage[item.ItemData.Icon], Vector2.One * ImGui.GetTextLineHeight());
            ImGui.SameLine();
            using var color = ImGuiRaii.PushColor(ImGuiCol.Text,
                time.Start > GatherBuddy.Time.ServerTime ? ColorId.UpcomingItem.Value() : ColorId.AvailableItem.Value(),
                !time.Equals(TimeInterval.Always));
            if (ImGui.Selectable(item.Name[GatherBuddy.Language], false))
                _plugin.Executor.GatherItem(item);
            color.Pop();
            CreateTooltip(loc, time);
        }

        DrawTime(loc, time);
    }

    private void DrawAlarms()
    {
        if (!GatherBuddy.Config.ShowGatherWindowAlarms)
            return;

        DrawAlarm(_plugin.AlarmManager.LastItemAlarm);
        DrawAlarm(_plugin.AlarmManager.LastFishAlarm);
    }


    public void Draw()
    {
        if (!GatherBuddy.Config.ShowGatherWindow
         || GatherBuddy.Config.OnlyShowGatherWindowHoldingCtrl && !ImGui.GetIO().KeyCtrl
         || GatherBuddy.Config.HideGatherWindowInDuty && Functions.BoundByDuty())
            return;

        var alarmTimers = GatherBuddy.Config.ShowGatherWindowAlarms
         && (_plugin.AlarmManager.LastFishAlarm != null || _plugin.AlarmManager.LastItemAlarm != null);

        if (_plugin.GatherWindowManager.ActiveItems.Count == 0 && !alarmTimers)
            return;

        using var color = ImGuiRaii.PushColor(ImGuiCol.WindowBg, ColorId.FishTimerBackground.Value());
        using var style = ImGuiRaii.PushStyle(ImGuiStyleVar.WindowPadding, Vector2.One * 2 * ImGuiHelpers.GlobalScale);
        ImGui.SetNextWindowSizeConstraints(Vector2.Zero, Vector2.One * 10000);
        var flags = ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar;
        if (GatherBuddy.Config.LockGatherWindow)
            flags |= ImGuiWindowFlags.NoMove;

        if (!ImGui.Begin("##GatherHelper", flags))
        {
            ImGui.End();
            return;
        }

        using var end = ImGuiRaii.DeferredEnd(ImGui.End);
        if (!ImGui.BeginTable("##table", GatherBuddy.Config.ShowGatherWindowTimers ? 2 : 1))
            return;

        end.Push(ImGui.EndTable);

        DrawAlarms();
        foreach (var item in _plugin.GatherWindowManager.GetList())
            DrawItem(item);
    }
}
