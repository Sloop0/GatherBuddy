﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using GatherBuddy.Caching;
using GatherBuddy.Classes;
using GatherBuddy.Enums;
using GatherBuddy.Interfaces;
using GatherBuddy.Time;
using ImGuiNET;
using ImGuiOtter;
using ImGuiScene;

namespace GatherBuddy.Gui;

public partial class Interface
{
    public class ExtendedFish
    {
        public struct BaitOrder
        {
            public TextureWrap    Icon;
            public string         Name;
            public object?        Fish;
            public TextureWrap?   HookSet;
            public (string, uint) Bite;
        }

        public struct Predator
        {
            public TextureWrap Icon;
            public string      Name;
            public string      Amount;
        }

        public static class Bites
        {
            public static readonly (string, uint) Weak      = ("  !  ", Colors.FishTab.WeakBite);
            public static readonly (string, uint) Strong    = (" ! ! ", Colors.FishTab.StrongBite);
            public static readonly (string, uint) Legendary = (" !!! ", Colors.FishTab.LegendaryBite);
            public static readonly (string, uint) Unknown   = (" ? ? ", Colors.FishTab.UnknownBite);

            public static (string, uint) FromBiteType(BiteType bite)
                => bite switch
                {
                    BiteType.Weak      => Weak,
                    BiteType.Strong    => Strong,
                    BiteType.Legendary => Legendary,
                    _                  => Unknown,
                };
        }

        public Fish        Data;
        public TextureWrap Icon;
        public string      Territories;
        public string      SpotNames;
        public string      Aetherytes;

        public string        Time;
        public TextureWrap[] WeatherIcons;
        public TextureWrap[] TransitionIcons;
        public BaitOrder[]   Bait;
        public TextureWrap?  Snagging;
        public Predator[]    Predators;
        public string        Patch;
        public string        UptimeString;
        public string        Intuition;
        public string        FishType;
        public bool          UptimeDependency;
        public ushort        UptimePercent;

        public (ILocation, TimeInterval) Uptime
            => GatherBuddy.UptimeManager.BestLocation(Data);

        private static ushort SetUptime(Fish fish)
        {
            var uptime     = 10000L;
            if (!fish.Interval.AlwaysUp())
                uptime = (uptime * fish.Interval.OnTime) / EorzeaTimeStampExtensions.MillisecondsPerEorzeaHour / RealTime.HoursPerDay;
            ushort bestUptime = 0;
            if (fish.ItemId == 8772)
                bestUptime = bestUptime;
            foreach (var spot in fish.FishingSpots)
            {
                var tmp = uptime
                  * spot.Territory.WeatherRates.ChanceForWeather(fish.PreviousWeather)
                  * spot.Territory.WeatherRates.ChanceForWeather(fish.CurrentWeather)
                  / 10000;
                if (tmp > bestUptime)
                    bestUptime = (ushort)tmp;
            }

            return bestUptime;
        }

        private static TextureWrap[] SetWeather(Fish fish)
        {
            if (!fish.FishRestrictions.HasFlag(FishRestrictions.Weather) || fish.CurrentWeather.Length == 0)
                return Array.Empty<TextureWrap>();

            return fish.CurrentWeather.Select(w => Icons.DefaultStorage[(uint)w.Data.Icon]).ToArray();
        }

        private static TextureWrap[] SetTransition(Fish fish)
        {
            if (!fish.FishRestrictions.HasFlag(FishRestrictions.Weather) || fish.PreviousWeather.Length == 0)
                return Array.Empty<TextureWrap>();

            return fish.PreviousWeather.Select(w => Icons.DefaultStorage[(uint)w.Data.Icon]).ToArray();
        }

        private static Predator[] SetPredators(Fish fish)
        {
            if (fish.Predators.Length == 0)
                return Array.Empty<Predator>();

            return fish.Predators.Select(p => new Predator
            {
                Amount = p.Item2.ToString(),
                Name   = p.Item1.Name[GatherBuddy.Language],
                Icon   = Icons.DefaultStorage[p.Item1.ItemData.Icon],
            }).ToArray();
        }

        private static BaitOrder[] SetBait(Fish fish)
        {
            if (fish.IsSpearFish)
                return new BaitOrder[]
                {
                    new()
                    {
                        Name    = string.Intern($"{fish.Size.ToName()} and {fish.Speed.ToName()}"),
                        Fish    = null,
                        Icon    = IconId.FromSize(fish.Size),
                        Bite    = Bites.Unknown,
                        HookSet = null,
                    },
                };

            var ret  = new BaitOrder[fish.Mooches.Length + 1];
            var bait = fish.InitialBait;
            ret[0] = new BaitOrder()
            {
                Icon = Icons.DefaultStorage[bait.Data.Icon],
                Name = bait.Name,
                Fish = bait,
            };
            for (var idx = 0; idx < fish.Mooches.Length; ++idx)
            {
                var f = fish.Mooches[idx];
                ret[idx].HookSet = IconId.FromHookSet(f.HookSet);
                ret[idx].Bite    = Bites.FromBiteType(f.BiteType);
                ret[idx + 1] = new BaitOrder()
                {
                    Icon = Icons.DefaultStorage[f.ItemData.Icon],
                    Name = f.Name[GatherBuddy.Language],
                    Fish = f,
                };
            }

            ret[^1].HookSet = IconId.FromHookSet(fish.HookSet);
            ret[^1].Bite    = Bites.FromBiteType(fish.BiteType);
            return ret;
        }

        private static TextureWrap? SetSnagging(Fish fish, IEnumerable<BaitOrder> baitOrder)
        {
            if (fish.Snagging == Enums.Snagging.Required)
                return IconId.GetSnagging();

            return baitOrder.Any(bait => bait.Fish is Fish { Snagging: Enums.Snagging.Required })
                ? IconId.GetSnagging()
                : null;
        }

        private static bool SetUptimeDependency(Fish fish, IEnumerable<BaitOrder> baitOrder)
        {
            bool CheckRestrictions(Fish f)
            {
                // naive check because exhaustive is complicated.
                if (f.FishRestrictions.HasFlag(FishRestrictions.Time) && f.Interval != fish.Interval)
                    return true;

                if (f.FishRestrictions.HasFlag(FishRestrictions.Weather))
                {
                    if (f.CurrentWeather.Intersect(fish.CurrentWeather).Count() < fish.CurrentWeather.Length)
                        return true;

                    if (f.PreviousWeather.Intersect(fish.PreviousWeather).Count() < fish.PreviousWeather.Length)
                        return true;
                }

                return false;
            }

            foreach (var bait in baitOrder)
            {
                if (bait.Fish is not Fish f)
                    continue;

                if (CheckRestrictions(f))
                    return true;
            }

            return fish.Predators.Any(p => CheckRestrictions(p.Item1));
        }

        private static string SetIntuition(Fish data)
        {
            var intuition = data.IntuitionLength;
            if (intuition <= 0)
                return string.Empty;

            var minutes = intuition / RealTime.SecondsPerMinute;
            var seconds = intuition % RealTime.SecondsPerMinute;
            if (seconds == 0)
                return minutes == 1 ? $"Intuition for {minutes} Minute" : $"Intuition for {minutes} Minutes";

            return $"Intuition for {minutes}:{seconds:D2} Minutes";
        }

        public ExtendedFish(Fish data)
        {
            Data        = data;
            Icon        = Icons.DefaultStorage[data.ItemData.Icon];
            Territories = string.Join("\n", data.FishingSpots.Select(f => f.Territory.Name).Distinct());
            if (!Territories.Contains("\n"))
                Territories = '\0' + Territories;
            SpotNames = string.Join("\n", data.FishingSpots.Select(f => f.Name).Distinct());
            if (!SpotNames.Contains("\n"))
                SpotNames = '\0' + SpotNames;
            Aetherytes = string.Join("\n",
                data.FishingSpots.Where(f => f.ClosestAetheryte != null).Select(f => f.ClosestAetheryte!.Name).Distinct());
            if (!Aetherytes.Contains("\n"))
                Aetherytes = '\0' + Aetherytes;
            Patch = $"Patch {data.Patch.ToVersionString()}";
            FishType = data.OceanFish ? string.Intern("Ocean Fish") :
                data.IsSpearFish      ? string.Intern("Spearfishing") :
                data.IsBigFish        ? string.Intern("Big Fish") : string.Intern("Regular Fish");

            Time = !data.FishRestrictions.HasFlag(FishRestrictions.Time)
                ? string.Intern("Always Up")
                : data.Interval.AlwaysUp()
                    ? string.Intern("Unknown Uptime")
                    : data.Interval.PrintHours();

            UptimePercent = SetUptime(data);
            UptimeString  = $"{(UptimePercent / 100f).ToString("F1", CultureInfo.InvariantCulture)}%%";
            if (UptimeString == "0.0%%")
                UptimeString = "<0.1%%";
            WeatherIcons     = SetWeather(data);
            TransitionIcons  = SetTransition(data);
            Predators        = SetPredators(data);
            Bait             = SetBait(data);
            Snagging         = SetSnagging(data, Bait);
            UptimeDependency = SetUptimeDependency(data, Bait);
            Intuition        = SetIntuition(data);
        }

        private static void PrintTime(ExtendedFish fish)
            => ImGuiUtil.DrawTextButton(fish.Time, Vector2.Zero, Colors.FishTab.Time);

        private static void PrintWeather(ExtendedFish fish, Vector2 weatherIconSize)
        {
            if (!fish.Data.FishRestrictions.HasFlag(FishRestrictions.Weather))
            {
                ImGuiUtil.DrawTextButton("No Weather Restrictions", Vector2.Zero, Colors.FishTab.Weather);
                return;
            }

            if (fish.WeatherIcons.Length == 0 && fish.TransitionIcons.Length == 0)
            {
                ImGuiUtil.DrawTextButton("Unknown Weather Restrictions", Vector2.Zero, Colors.FishTab.Weather);
                return;
            }

            using var style = ImGuiRaii.PushStyle(ImGuiStyleVar.ItemSpacing, ImGui.GetStyle().ItemSpacing / 2);
            if (fish.TransitionIcons.Length > 0)
            {
                AlignTextToSize(fish.TransitionIcons.Length > 1 ? "Requires one of" : "Requires", weatherIconSize);
                style.Push(ImGuiStyleVar.ItemSpacing, Vector2.One * ImGuiHelpers.GlobalScale);
                foreach (var w in fish.TransitionIcons)
                {
                    ImGui.SameLine();
                    ImGui.Image(w.ImGuiHandle, weatherIconSize);
                }

                style.Pop();

                ImGui.SameLine();
                AlignTextToSize(fish.WeatherIcons.Length > 1 ? "followed by one of" : "followed by", weatherIconSize);
                if (fish.WeatherIcons.Length == 0)
                {
                    ImGui.SameLine();
                    AlignTextToSize(" Anything", weatherIconSize);
                }
                else
                {
                    style.Push(ImGuiStyleVar.ItemSpacing, Vector2.One * ImGuiHelpers.GlobalScale);
                    foreach (var w in fish.WeatherIcons)
                    {
                        ImGui.SameLine();
                        ImGui.Image(w.ImGuiHandle, weatherIconSize);
                    }
                }
            }
            else if (fish.WeatherIcons.Length > 0)
            {
                AlignTextToSize(fish.WeatherIcons.Length > 1 ? "Requires one of" : "Requires", weatherIconSize);
                style.Push(ImGuiStyleVar.ItemSpacing, Vector2.One * ImGuiHelpers.GlobalScale);
                foreach (var w in fish.WeatherIcons)
                {
                    ImGui.SameLine();
                    ImGui.Image(w.ImGuiHandle, weatherIconSize);
                }
            }
        }

        private static void PrintBait(ExtendedFish fish, Vector2 iconSize, Vector2 smallIconSize)
        {
            if (fish.Bait.Length == 0)
            {
                ImGuiUtil.DrawTextButton("Unknown Catch Method", Vector2.Zero, Colors.FishTab.Bait);
                return;
            }

            using var style = ImGuiRaii.PushStyle(ImGuiStyleVar.ItemSpacing, ImGui.GetStyle().ItemSpacing / 2);

            var startPos = ImGui.GetCursorPos();
            var size     = Vector2.Zero;
            if (fish.Snagging != null)
            {
                ImGui.Image(fish.Snagging.ImGuiHandle, iconSize);
                ImGui.SameLine();
            }

            foreach (var bait in fish.Bait)
            {
                size = iconSize;
                ImGui.Image(bait.Icon.ImGuiHandle, size);

                if (!fish.Data.IsSpearFish)
                {
                    style.Push(ImGuiStyleVar.ItemSpacing, Vector2.One);
                    ImGui.SameLine();
                    using var _ = ImGuiRaii.NewGroup();
                    style.Push(ImGuiStyleVar.FramePadding, Vector2.Zero);
                    ImGui.Image(bait.HookSet!.ImGuiHandle, smallIconSize);
                    using var color = ImGuiRaii.PushColor(ImGuiCol.Button, bait.Bite.Item2);
                    ImGui.Button(bait.Bite.Item1, smallIconSize);
                    style.Pop(2);
                }

                ImGui.SameLine();

                var pos = ImGui.GetCursorPosY();
                ImGui.SetCursorPosY(pos + (iconSize.Y - ImGui.GetTextLineHeight()) / 2);
                ImGui.Text(bait.Name);
                if (bait.Equals(fish.Bait.Last()))
                    break;

                ImGui.SameLine();
                ImGui.Text(" → ");
                ImGui.SameLine();
                ImGui.SetCursorPosY(pos);
            }

            ImGui.SetCursorPos(startPos + new Vector2(0, size.Y + ImGui.GetStyle().ItemSpacing.Y));
        }

        private static void PrintPredators(ExtendedFish fish, Vector2 iconSize)
        {
            if (fish.Predators.Length == 0 && fish.Intuition.Length == 0)
                return;

            var size   = iconSize / 1.5f;
            var offset = (size.Y - ImGui.GetTextLineHeight()) / 2f;
            var length = ImGui.CalcTextSize(fish.Intuition).X;

            using var color = ImGuiRaii.PushColor(ImGuiCol.Button, Colors.FishTab.Predator);
            using var style = ImGuiRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.One);
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


            if (fish.Intuition.Length == 0)
                return;

            color.Push(ImGuiCol.Button, Colors.FishTab.Intuition);
            ImGui.Button(fish.Intuition);
        }

        private static void PrintFolklore(ExtendedFish fish)
        {
            using var color = new ImGuiRaii.Color();
            if (fish.Data.Folklore.Length != 0)
            {
                color.Push(ImGuiCol.Button, Colors.FishTab.Folklore);
                ImGui.Button(fish.Data.Folklore);
                color.Pop();
                ImGui.SameLine();
            }

            color.Push(ImGuiCol.Button, Colors.FishTab.Patch)
                .Push(ImGuiCol.Text, Colors.FishTab.PatchText);
            ImGui.Button(fish.Patch);
        }

        public void SetTooltip(Vector2 iconSize, Vector2 smallIconSize, Vector2 weatherIconSize)
        {
            using var tooltip = ImGuiRaii.NewTooltip();
            using var style   = ImGuiRaii.PushStyle(ImGuiStyleVar.ItemSpacing, ImGui.GetStyle().ItemSpacing * new Vector2(1f, 1.5f));
            PrintTime(this);
            PrintWeather(this, weatherIconSize);
            PrintBait(this, iconSize, smallIconSize);
            PrintPredators(this, iconSize);
            PrintFolklore(this);
        }

        public void Draw(Vector2 lineIconSize, Vector2 iconSize, Vector2 smallIconSize, Vector2 weatherIconSize)
        {
            var (location, uptime) = Uptime;

            ImGui.TableNextColumn();
            ImGuiUtil.HoverIcon(Icon, lineIconSize);
            ImGui.SameLine();
            ImGui.Selectable(Data.Name[GatherBuddy.Language]);
            if (ImGui.IsItemHovered())
                SetTooltip(iconSize, smallIconSize, weatherIconSize);
            ImGui.TableNextColumn();
            DrawTimeInterval(uptime, UptimeDependency);

            //ImGui.TableNextColumn();
            //ImGui.Text(Level);
            //
            //ImGui.TableNextColumn();
            //ImGui.Text(Data.GatheringType.ToString());
            //ImGui.TableNextColumn();
            //ImGui.Text(Data.NodeType.ToString());
            //ImGui.TableNextColumn();
            //ImGui.Text(Uptimes);
            //ImGui.TableNextColumn();
            //ImGui.Text(Folklore);
            //
            //ImGui.TableNextColumn();
            //ImGui.Selectable(location.Name);
            //ImGuiUtil.HoverTooltip($"All locations:\n{string.Join("\n", Data.NodeList.Select(n => n.Name))}");
            //
            //ImGui.TableNextColumn();
            //ImGui.Text(location.Territory.Name);
            //ImGuiUtil.HoverTooltip($"All zones:\n{string.Join("\n", Data.NodeList.Select(n => n.Territory.Name).Distinct())}");
            //
            //ImGui.TableNextColumn();
            //ImGui.Selectable(location.ClosestAetheryte?.Name ?? "None");
        }
    }
}
