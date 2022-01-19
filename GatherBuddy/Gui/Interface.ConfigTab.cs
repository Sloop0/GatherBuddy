﻿using System;
using Dalamud.Interface;
using GatherBuddy.Config;
using ImGuiNET;
using ImGuiOtter;

namespace GatherBuddy.Gui;

public partial class Interface
{
    private static class ConfigFunctions
    {
        public static void DrawSetInput(string jobName, string oldName, Action<string> setName)
        {
            var tmp = oldName;
            ImGui.SetNextItemWidth(SetInputWidth);
            if (ImGui.InputText($"{jobName} Set", ref tmp, 15) && tmp != oldName)
            {
                setName(tmp);
                GatherBuddy.Config.Save();
            }

            ImGuiUtil.HoverTooltip($"Set the name of your {jobName.ToLowerInvariant()} set. Can also be the numerical id instead.");
        }

        private static void DrawCheckbox(string label, string description, bool oldValue, Action<bool> setter)
        {
            if (ImGuiUtil.Checkbox(label, description, oldValue, setter))
                GatherBuddy.Config.Save();
        }

        public static ItemFilter SetShowItems(ItemFilter oldValue, ItemFilter flag, bool value)
        {
            if (value)
                return oldValue | flag;

            return oldValue & ~flag;
        }

        public static FishFilter SetShowFish(FishFilter oldValue, FishFilter flag, bool value)
        {
            if (value)
                return oldValue | flag;

            return oldValue & ~flag;
        }

        public static void DrawVisibilityBox<T>(T flag, T oldValue, string label, string tooltip, Action<T, bool> setFlag) where T : Enum
        {
            var tmp = oldValue.HasFlag(flag);
            if (ImGui.Checkbox(label, ref tmp))
            {
                setFlag(flag, tmp);
                GatherBuddy.Config.Save();
            }

            ImGuiUtil.HoverTooltip(tooltip);
        }


        // General Config
        public static void DrawOpenOnStartBox()
            => DrawCheckbox("Open Config UI On Start",
                "Toggle whether the GatherBuddy GUI should be visible after you start the game.",
                GatherBuddy.Config.OpenOnStart, b => GatherBuddy.Config.OpenOnStart = b);

        public static void DrawGearChangeBox()
            => DrawCheckbox("Enable Gear Change",
                "Toggle whether to automatically switch gear to the correct job gear for a node.\nUses Miner Set, Botanist Set and Fisher Set.",
                GatherBuddy.Config.UseGearChange, b => GatherBuddy.Config.UseGearChange = b);

        public static void DrawTeleportBox()
            => DrawCheckbox("Enable Teleport",
                "Toggle whether to automatically teleport to a chosen node.",
                GatherBuddy.Config.UseTeleport, b => GatherBuddy.Config.UseTeleport = b);

        public static void DrawMapMarkerOpenBox()
            => DrawCheckbox("Open Map With Marker",
                "Toggle whether to automatically set a map marker on the approximate location of the chosen node and open the map of that territory.",
                GatherBuddy.Config.UseCoordinates, b => GatherBuddy.Config.UseCoordinates = b);

        public static void DrawMapMarkerPrintBox()
            => DrawCheckbox("Print Map Location",
                "Toggle whether to automatically write a map link to the approximate location of the chosen node to chat.",
                GatherBuddy.Config.WriteCoordinates, b => GatherBuddy.Config.WriteCoordinates = b);

        public static void DrawPrintUptimesBox()
            => DrawCheckbox("Print Node Uptimes On Gather",
                "Print the uptimes of nodes you try to /gather in the chat if they are not always up.",
                GatherBuddy.Config.PrintUptime, b => GatherBuddy.Config.PrintUptime = b);

        public static void DrawPrintSpearfishBox()
            => DrawCheckbox("Print Spear Fish Info On Gather",
                "Print the size and speed of the fish you try to /gatherfish, if it is obtained via spearfishing.",
                GatherBuddy.Config.PrintSpearfishInfo, b => GatherBuddy.Config.PrintSpearfishInfo = b);

        public static void DrawSkipTeleportBox()
            => DrawCheckbox("Skip Nearby Teleports",
                "Skips teleports if you are in the same map and closer to the target than the selected aetheryte already.",
                GatherBuddy.Config.SkipTeleportIfClose, b => GatherBuddy.Config.SkipTeleportIfClose = b);


        // Weather Tab
        public static void DrawWeatherTabNamesBox()
            => DrawCheckbox("Show Names in Weather Tab",
                "Toggle whether to write the names in the table for the weather tab, or just the icons with names on hover.",
                GatherBuddy.Config.ShowWeatherNames, b => GatherBuddy.Config.ShowWeatherNames = b);

        // Alarms
        public static void DrawAlarmToggle()
            => DrawCheckbox("Enable Alarms", "Toggle all alarms on or off.", GatherBuddy.Config.AlarmsEnabled,
                b => { GatherBuddy.Config.AlarmsEnabled = b; }); // TODO when alarms exist

        // Fish Timer
        public static void DrawFishTimerBox()
            => DrawCheckbox("Show Fish Timer",
                "Toggle whether to show the fish timer window while fishing.",
                GatherBuddy.Config.ShowFishTimer, b => GatherBuddy.Config.ShowFishTimer = b);

        public static void DrawFishTimerEditBox()
            => DrawCheckbox("Edit Fish Timer",
                "Enable editing the fish timer window.",
                GatherBuddy.Config.FishTimerEdit, b => GatherBuddy.Config.FishTimerEdit = b);

        public static void DrawFishTimerHideBox()
            => DrawCheckbox("Hide Uncaught Fish in Fish Timer",
                "Hide all fish from the fish timer window that have not been recorded with the given combination of snagging and bait.",
                GatherBuddy.Config.HideUncaughtFish, b => GatherBuddy.Config.HideUncaughtFish = b);

        public static void DrawFishTimerHideBox2()
            => DrawCheckbox("Hide Unavailable Fish in Fish Timer",
                "Hide all fish from the fish timer window that have have known requirements that are unfulfilled, like Fisher's Intuition or Snagging.",
                GatherBuddy.Config.HideUnavailableFish, b => GatherBuddy.Config.HideUnavailableFish = b);

        public static void DrawFishTimerUptimesBox()
            => DrawCheckbox("Show Uptimes in Fish Timer",
                "Show the uptimes for restricted fish in the fish timer window.",
                GatherBuddy.Config.ShowFishTimerUptimes, b => GatherBuddy.Config.ShowFishTimerUptimes = b);

        // Gather Window
        public static void DrawShowGatherWindowBox()
            => DrawCheckbox("Show Gather Window",
                "Show a small window with pinned Gatherables and their uptimes.",
                GatherBuddy.Config.ShowGatherWindow, b => GatherBuddy.Config.ShowGatherWindow = b);

        public static void DrawGatherWindowTimersBox()
            => DrawCheckbox("Show Gather Window Timers",
                "Show the uptimes for gatherables in the gather window.",
                GatherBuddy.Config.ShowGatherWindowTimers, b => GatherBuddy.Config.ShowGatherWindowTimers = b);

        public static void DrawGatherWindowAlarmsBox()
            => DrawCheckbox("Show Last Alarms in Gather Window",
                "Show the last triggered alarms in your gather window if they exist.",
                GatherBuddy.Config.ShowGatherWindowAlarms, b => GatherBuddy.Config.ShowGatherWindowAlarms = b);

        public static void DrawAetherytePreference()
        {
            var tmp     = GatherBuddy.Config.AetherytePreference == AetherytePreference.Cost;
            var oldPref = GatherBuddy.Config.AetherytePreference;
            if (ImGui.RadioButton("Prefer Cheaper Aetherytes", tmp))
                GatherBuddy.Config.AetherytePreference = AetherytePreference.Cost;
            var hovered = ImGui.IsItemHovered();
            ImGui.SameLine();
            if (ImGui.RadioButton("Prefer Less Travel Time", !tmp))
                GatherBuddy.Config.AetherytePreference = AetherytePreference.Distance;
            hovered |= ImGui.IsItemHovered();
            if (hovered)
                ImGui.SetTooltip(
                    "Specify whether you prefer aetherytes that are closer to your target (less travel time)"
                  + " or aetherytes that are cheaper to teleport to when scanning through all available nodes for an item."
                  + " Only matters if the item is not timed and has multiple sources.");

            if (oldPref != GatherBuddy.Config.AetherytePreference)
            {
                GatherBuddy.UptimeManager.ResetLocations();
                GatherBuddy.Config.Save();
            }
        }
    }

    private void DrawConfigTab()
    {
        using var id = ImGuiRaii.PushId("Config");
        if (!ImGui.BeginTabItem("Config"))
            return;

        using var end = ImGuiRaii.DeferredEnd(ImGui.EndTabItem);

        if (ImGui.CollapsingHeader("General"))
        {
            ConfigFunctions.DrawGearChangeBox();
            ConfigFunctions.DrawTeleportBox();
            ConfigFunctions.DrawMapMarkerOpenBox();
            ConfigFunctions.DrawAetherytePreference();
            ConfigFunctions.DrawSkipTeleportBox();
            ConfigFunctions.DrawAlarmToggle();
            ImGui.NewLine();
        }

        if (ImGui.CollapsingHeader("Set Names"))
        {
            ConfigFunctions.DrawSetInput("Miner",    GatherBuddy.Config.MinerSetName, s => GatherBuddy.Config.MinerSetName    = s);
            ConfigFunctions.DrawSetInput("Botanist", GatherBuddy.Config.BotanistSetName,     s => GatherBuddy.Config.BotanistSetName = s);
            ConfigFunctions.DrawSetInput("Fisher",   GatherBuddy.Config.FisherSetName,       s => GatherBuddy.Config.FisherSetName   = s);
            ImGui.NewLine();
        }

        if (ImGui.CollapsingHeader("Interface"))
        {
            ConfigFunctions.DrawOpenOnStartBox();
            ConfigFunctions.DrawWeatherTabNamesBox();
            ConfigFunctions.DrawFishTimerBox();
            ConfigFunctions.DrawFishTimerEditBox();
            ConfigFunctions.DrawFishTimerHideBox();
            ConfigFunctions.DrawFishTimerHideBox2();
            ConfigFunctions.DrawFishTimerUptimesBox();
            ConfigFunctions.DrawShowGatherWindowBox();
            ConfigFunctions.DrawGatherWindowTimersBox();
            ConfigFunctions.DrawGatherWindowAlarmsBox();
            ImGui.NewLine();
        }

        if (ImGui.CollapsingHeader("Messages"))
        {
            ConfigFunctions.DrawMapMarkerPrintBox();
            ConfigFunctions.DrawPrintUptimesBox();
            ConfigFunctions.DrawPrintSpearfishBox();
            ImGui.NewLine();
        }

        if (ImGui.CollapsingHeader("Colors"))
        {
            foreach (var color in Enum.GetValues<ColorId>())
            {
                var (defaultColor, name, description) = color.Data();
                var currentColor = GatherBuddy.Config.Colors.TryGetValue(color, out var current) ? current : defaultColor;
                if (ImGuiUtil.ColorPicker(name, description, currentColor, c => GatherBuddy.Config.Colors[color] = c, defaultColor))
                    GatherBuddy.Config.Save();
            }

            ImGui.NewLine();
        }
    }
}
