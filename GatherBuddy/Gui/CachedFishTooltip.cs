﻿using System;
using System.Linq;
using System.Numerics;
using GatherBuddy.Caching;
using GatherBuddy.Enums;
using ImGuiNET;

namespace GatherBuddy.Gui;

public partial class Interface
{
    private static void PrintTime(ExtendedFish fish)
        => DrawButtonText(fish.Time, Vector2.Zero, Colors.FishTab.Time);

    private static void PrintWeather(ExtendedFish fish)
    {
        if (!fish.Fish.FishRestrictions.HasFlag(FishRestrictions.Weather))
        {
            DrawButtonText("No Weather Restrictions", Vector2.Zero, Colors.FishTab.Weather);
        }
        else if (fish.WeatherIcons.Length == 0)
        {
            DrawButtonText("Unknown Weather Restrictions", Vector2.Zero, Colors.FishTab.Weather);
        }
        else
        {
            Vector2 pos;
            var     space = _itemSpacing.X / 2;
            if (fish.WeatherIcons[0].Length > 0)
            {
                pos   =  AlignedTextToWeatherIcon(fish.WeatherIcons[0].Length > 1 ? "Requires one of" : "Requires");
                pos.X -= space;
                foreach (var w in fish.WeatherIcons[0])
                {
                    ImGui.SetCursorPos(pos);
                    pos.X += _iconSize.X;
                    ImGui.Image(w.ImGuiHandle, _weatherIconSize);
                }

                pos.X += space;
                ImGui.SetCursorPos(pos);
                pos   =  AlignedTextToWeatherIcon(fish.WeatherIcons[1].Length > 1 ? "followed by one of" : "followed by");
                pos.X -= space;
            }
            else
            {
                pos   =  AlignedTextToWeatherIcon(fish.WeatherIcons[1].Length > 1 ? "Requires one of" : "Requires");
                pos.X -= space;
            }

            if (fish.WeatherIcons[1].Length == 0)
            {
                ImGui.SetCursorPos(pos);
                AlignedTextToWeatherIcon(Colors.FishTab.WeatherVec4, " Anything");
            }
            else
            {
                foreach (var w in fish.WeatherIcons[1])
                {
                    ImGui.SetCursorPos(pos);
                    pos.X += _iconSize.X;
                    ImGui.Image(w.ImGuiHandle, _weatherIconSize);
                }
            }
        }
    }

    private static void PrintBait(ExtendedFish fish)
    {
        if (fish.Bait.Length == 0)
        {
            DrawButtonText("Unknown Catch Method", Vector2.Zero, Colors.FishTab.Bait);
            return;
        }

        using var imgui = new ImGuiRaii().PushStyle(ImGuiStyleVar.ItemSpacing, _itemSpacing / 2);

        var startPos = ImGui.GetCursorPos();
        var size     = Vector2.Zero;
        if (fish.Snagging != null)
        {
            ImGui.Image(fish.Snagging.ImGuiHandle, _iconSize);
            ImGui.SameLine();
        }

        foreach (var bait in fish.Bait)
        {
            size = _iconSize;
            ImGui.Image(bait.Icon.ImGuiHandle, size);

            if (!fish.Fish.IsSpearFish)
            {
                using var imgui2 = new ImGuiRaii()
                    .PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.One);
                ImGui.SameLine();
                imgui2.Group();
                imgui2.PushStyle(ImGuiStyleVar.FramePadding, Vector2.Zero);
                ImGui.Image(bait.HookSet!.ImGuiHandle, _smallIconSize);
                imgui2.PushColor(ImGuiCol.Button, bait.Bite.Item2);
                ImGui.Button(bait.Bite.Item1, _smallIconSize);
            }

            ImGui.SameLine();

            var pos = ImGui.GetCursorPosY();
            ImGui.SetCursorPosY(pos + (_iconSize.Y - ImGui.GetTextLineHeight()) / 2);
            ImGui.Text(bait.Name);
            if (bait.Equals(fish.Bait.Last()))
                break;

            ImGui.SameLine();
            ImGui.Text(" → ");
            ImGui.SameLine();
            ImGui.SetCursorPosY(pos);
        }

        ImGui.SetCursorPos(startPos + new Vector2(0, size.Y + _itemSpacing.Y));
    }

    private static void PrintPredators(ExtendedFish fish)
    {
        if (fish.Predators.Length == 0 && fish.IntuitionText.Length == 0)
            return;

        var size   = _iconSize / 1.5f;
        var offset = (size.Y - _textHeight) / 2f;
        var length = ImGui.CalcTextSize(fish.IntuitionText).X;

        using (var _ = new ImGuiRaii()
                   .PushColor(ImGuiCol.Button, Colors.FishTab.Predator)
                   .PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.One))
        {
            foreach (var predator in fish.Predators)
            {
                using var group = ImGuiRaii.NewGroup();
                ImGui.Button(predator.Amount, size);
                ImGui.SameLine();
                ImGui.Image(predator.Icon.ImGuiHandle, size);
                ImGui.SameLine();
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + offset);
                ImGui.Text(predator.Name);
                ImGui.SameLine();
                length = Math.Max(length, ImGui.GetCursorPosX());
                ImGui.NewLine();
            }
        }


        if (fish.IntuitionText.Length == 0)
            return;

        using var intuition = new ImGuiRaii()
            .PushColor(ImGuiCol.Button, Colors.FishTab.Intuition);
        ImGui.Button(fish.IntuitionText);
    }

    private static void PrintFolklore(ExtendedFish fish)
    {
        using var imgui = new ImGuiRaii();
        if (fish.Fish.Folklore.Length != 0)
        {
            imgui.PushColor(ImGuiCol.Button, Colors.FishTab.Folklore);
            ImGui.Button(fish.Fish.Folklore);
            imgui.PopColors();
            ImGui.SameLine();
        }

        imgui.PushColor(ImGuiCol.Button, Colors.FishTab.Patch)
            .PushColor(ImGuiCol.Text, Colors.FishTab.PatchText);
        ImGui.Button(fish.Patch);
    }

    private static void SetTooltip(ExtendedFish fish)
    {
        using var tooltip = ImGuiRaii.NewTooltip()
            .PushStyle(ImGuiStyleVar.ItemSpacing, new Vector2(_itemSpacing.X, _itemSpacing.Y * 1.5f));
        PrintTime(fish);
        PrintWeather(fish);
        PrintBait(fish);
        PrintPredators(fish);
        PrintFolklore(fish);
    }
}
