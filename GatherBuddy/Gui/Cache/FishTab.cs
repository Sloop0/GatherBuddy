using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Dalamud.Logging;
using GatherBuddy.Caching;
using GatherBuddy.Classes;
using GatherBuddy.Enums;
using GatherBuddy.Time;
using GatherBuddy.Utility;
using GatherBuddy.Weather;
using ImGuiNET;
using Lumina.Data.Parsing.Uld;

namespace GatherBuddy.Gui.Cache;

internal class FishTab
{
    public static readonly string[]                   Patches       = PreparePatches();
    public static readonly Func<ExtendedFish, bool>[] PatchSelector = PreparePatchSelectors();

    public          List<ExtendedFish> RelevantFish = new();
    public          List<ExtendedFish> CachedFish   = new();
    public readonly List<ExtendedFish> FixedFish;

    public readonly float LongestFish;
    public readonly float LongestSpot;
    public readonly float LongestZone;
    public readonly float LongestBait;
    public readonly float LongestPercentage;
    public readonly float LongestMinutes;

    public  string FishFilter      = "";
    public  string FishFilterLower = "";
    public  string BaitFilter      = "";
    public  string BaitFilterLower = "";
    public  string SpotFilter      = "";
    public  string SpotFilterLower = "";
    public  string ZoneFilter      = "";
    public  string ZoneFilterLower = "";
    private byte   _whichFilters;
    public  bool   ResortFish = false;

    public byte WhichFilters
        => (byte)(_whichFilters | (GatherBuddy.Config.ShowAlreadyCaught ? 0 : 1));

    public void UpdateFishFilter()
    {
        FishFilterLower = FishFilter.ToLowerInvariant();
        if (FishFilter.Length > 0)
            _whichFilters |= 0b00010;
        else
            _whichFilters &= 0b11100;
    }

    public void UpdateBaitFilter()
    {
        BaitFilterLower = BaitFilter.ToLowerInvariant();
        if (BaitFilter.Length > 0)
            _whichFilters |= 0b00100;
        else
            _whichFilters &= 0b11010;
    }

    public void UpdateSpotFilter()
    {
        SpotFilterLower = SpotFilter.ToLowerInvariant();
        if (SpotFilter.Length > 0)
            _whichFilters |= 0b01000;
        else
            _whichFilters &= 0b10110;
    }

    public void UpdateZoneFilter()
    {
        ZoneFilterLower = ZoneFilter.ToLowerInvariant();
        if (ZoneFilter.Length > 0)
            _whichFilters |= 0b10000;
        else
            _whichFilters &= 0b01110;
    }

    public Func<ExtendedFish, bool> Selector;

    private readonly WeatherManager        _weather;
    private readonly FishManager    _fishManager;
    private readonly UptimeComparer _uptimeComparer = new();

    private static List<ExtendedFish> SetupFixedFish(IList<uint> fishIds, FishManager fishManager)
    {
        var bad = false;
        var ret = new List<ExtendedFish>(fishIds.Count);
        for (var i = 0; i < fishIds.Count; ++i)
        {
            var id = fishIds[i];
            if (!fishManager.FishInLog.TryGetValue(id, out var fish))
            {
                bad = true;
                fishIds.RemoveAt(i);
                --i;
                PluginLog.Information($"Removed invalid fish id {id} from fixed fishes.");
                continue;
            }

            ret.Add(fish);
            fish.IsFixed = true;
        }

        if (bad)
            GatherBuddy.Config.Save();

        return ret;
    }

    public FishTab(WeatherManager weather, FishManager fishManager, Icons icons)
    {
        _weather     = weather;
        _fishManager = fishManager;

        Selector = PatchSelector[GatherBuddy.Config.ShowFishFromPatch];

        foreach (var fish in _fishManager.FishInLog.Values)
        {
            LongestFish       = System.Math.Max(LongestFish,       ImGui.CalcTextSize(fish.Name).X / ImGui.GetIO().FontGlobalScale);
            LongestSpot       = System.Math.Max(LongestSpot,       ImGui.CalcTextSize(fish.FishingSpot).X / ImGui.GetIO().FontGlobalScale);
            LongestZone       = System.Math.Max(LongestZone,       ImGui.CalcTextSize(fish.Territory).X / ImGui.GetIO().FontGlobalScale);
            LongestPercentage = System.Math.Max(LongestPercentage, ImGui.CalcTextSize(fish.UptimeString).X / ImGui.GetIO().FontGlobalScale);
            if (fish.Bait.Length > 0)
                LongestBait = System.Math.Max(LongestBait, ImGui.CalcTextSize(fish.Bait[0].Name).X / ImGui.GetIO().FontGlobalScale);
        }

        LongestMinutes = ImGui.CalcTextSize("0000:00 Minutes").X / ImGui.GetIO().FontGlobalScale;
        FixedFish      = SetupFixedFish(GatherBuddy.Config.FixedFish, fishManager);

        SetCurrentlyRelevantFish();
    }

    public void FixFish(ExtendedFish fish)
    {
        if (FixedFish.Contains(fish))
            return;

        fish.IsFixed = true;
        FixedFish.Add(fish);
        GatherBuddy.Config.FixedFish.Add(fish.Fish.ItemId);
        GatherBuddy.Config.Save();
    }

    public void UnfixFish(ExtendedFish fish)
    {
        if (!FixedFish.Remove(fish))
            return;

        fish.IsFixed = false;
        GatherBuddy.Config.FixedFish.Remove(fish.Fish.ItemId);
        GatherBuddy.Config.Save();
    }

    public void ToggleFishFix(ExtendedFish fish)
    {
        if (FixedFish.Contains(fish))
        {
            UnfixFish(fish);
        }
        else
        {
            fish.IsFixed = true;
            FixedFish.Add(fish);
            GatherBuddy.Config.FixedFish.Add(fish.Fish.ItemId);
            GatherBuddy.Config.Save();
        }
    }

    public void UpdateFish()
    {
        if (!ResortFish)
            return;

        ResortFish = false;
        CachedFish = SortFish();
    }

    public void SortFish()
    {
        switch (GatherBuddy.Config.FishSortOder)
        {
            case FishSortOrder.EndTime:
                RelevantFish.Sort((f1, f2) => _uptimeComparer.Compare(f1.Fish.NextUptime(), f2.Fish.NextUptime()));
                return;
            case FishSortOrder.Name:
                RelevantFish.Sort((f1, f2) => string.Compare(f1.NameLower, f2.NameLower, StringComparison.InvariantCulture));
                return;
            case FishSortOrder.BaitName:
                RelevantFish.Sort((f1, f2) => string.Compare(f1.FirstBaitLower, f2.FirstBaitLower, StringComparison.InvariantCulture));
                return;
            case FishSortOrder.FishingSpotName:
                RelevantFish.Sort((f1, f2) => string.Compare(f1.FishingSpotLower, f2.FishingSpotLower, StringComparison.InvariantCulture));
                return;
            case FishSortOrder.ZoneName:
                RelevantFish.Sort((f1, f2) => string.Compare(f1.TerritoryLower, f2.TerritoryLower, StringComparison.InvariantCulture));
                return;
            case FishSortOrder.Uptime:
                RelevantFish.Sort((f1, f2) => f1.Uptime.CompareTo(f2.Uptime));
                return;
            case FishSortOrder.InverseEndTime:
                RelevantFish.Sort((f1, f2) => _uptimeComparer.Compare(f2.Fish.NextUptime(), f1.Fish.NextUptime()));
                return;
            case FishSortOrder.InverseName:
                RelevantFish.Sort((f1, f2) => string.Compare(f2.NameLower, f1.NameLower, StringComparison.InvariantCulture));
                return;
            case FishSortOrder.InverseBaitName:
                RelevantFish.Sort((f1, f2) => string.Compare(f2.FirstBaitLower, f1.FirstBaitLower, StringComparison.InvariantCulture));
                return;
            case FishSortOrder.InverseFishingSpotName:
                RelevantFish.Sort((f1, f2) => string.Compare(f2.FishingSpotLower, f1.FishingSpotLower, StringComparison.InvariantCulture));
                return;
            case FishSortOrder.InverseZoneName:
                RelevantFish.Sort((f1, f2) => string.Compare(f2.TerritoryLower, f1.TerritoryLower, StringComparison.InvariantCulture));
                return;
            case FishSortOrder.InverseUptime:
                RelevantFish.Sort((f1, f2) => f2.Uptime.CompareTo(f1.Uptime));
                return;
        }
    }


    public void SetCurrentlyRelevantFish()
    {
        RelevantFish = _fishManager.FishInLog.Values
            .Where(SelectFish)
            .ToList();
        SortFish();
        CachedFish = RelevantFish;
    }

    public List<ExtendedFish> GetFishToSettings()
    {
            // @formatter:off
            var filters = WhichFilters;
            if (FixedFish.Count > 0)
                filters |= 0b100000;
            return filters switch
            {
                0b000000 => CachedFish,
                0b000001 => CachedFish.Where(f => FishUncaught(f)).ToList(),
                0b000010 => CachedFish.Where(f => f.NameLower.Contains(FishFilterLower)).ToList(),
                0b000011 => CachedFish.Where(f => FishUncaught(f) && f.NameLower.Contains(FishFilterLower)).ToList(),
                0b000100 => CachedFish.Where(f => f.FirstBaitLower.Contains(BaitFilterLower)).ToList(),
                0b000101 => CachedFish.Where(f => FishUncaught(f) && f.FirstBaitLower.Contains(BaitFilterLower)).ToList(),
                0b000110 => CachedFish.Where(f => f.NameLower.Contains(FishFilterLower) && f.FirstBaitLower.Contains(BaitFilterLower)).ToList(),
                0b000111 => CachedFish.Where(f => FishUncaught(f) && f.NameLower.Contains(FishFilterLower) && f.FirstBaitLower.Contains(BaitFilterLower)).ToList(),
                0b001000 => CachedFish.Where(f => f.FishingSpotLower.Contains(SpotFilterLower)).ToList(),
                0b001001 => CachedFish.Where(f => FishUncaught(f) && f.FishingSpotLower.Contains(SpotFilterLower)).ToList(),
                0b001010 => CachedFish.Where(f => f.NameLower.Contains(FishFilterLower) && f.FishingSpotLower.Contains(SpotFilterLower)).ToList(),
                0b001011 => CachedFish.Where(f => FishUncaught(f) && f.NameLower.Contains(FishFilterLower) && f.FishingSpotLower.Contains(SpotFilterLower)).ToList(),
                0b001100 => CachedFish.Where(f => f.FirstBaitLower.Contains(BaitFilterLower) && f.FishingSpotLower.Contains(SpotFilterLower)).ToList(),
                0b001101 => CachedFish.Where(f => FishUncaught(f) && f.FirstBaitLower.Contains(BaitFilterLower) && f.FishingSpotLower.Contains(SpotFilterLower)).ToList(),
                0b001110 => CachedFish.Where(f => f.NameLower.Contains(FishFilterLower) && f.FirstBaitLower.Contains(BaitFilterLower) && f.FishingSpotLower.Contains(SpotFilterLower)).ToList(),
                0b001111 => CachedFish.Where(f => FishUncaught(f) && f.NameLower.Contains(FishFilterLower) && f.FirstBaitLower.Contains(BaitFilterLower) && f.FishingSpotLower.Contains(SpotFilterLower)).ToList(),
                0b010000 => CachedFish.Where(f => f.TerritoryLower.Contains(ZoneFilterLower)).ToList(),
                0b010001 => CachedFish.Where(f => FishUncaught(f) && f.TerritoryLower.Contains(ZoneFilterLower)).ToList(),
                0b010010 => CachedFish.Where(f => f.NameLower.Contains(FishFilterLower) && f.TerritoryLower.Contains(ZoneFilterLower)).ToList(),
                0b010011 => CachedFish.Where(f => FishUncaught(f) && f.NameLower.Contains(FishFilterLower) && f.TerritoryLower.Contains(ZoneFilterLower)).ToList(),
                0b010100 => CachedFish.Where(f => f.FirstBaitLower.Contains(BaitFilterLower) && f.TerritoryLower.Contains(ZoneFilterLower)).ToList(),
                0b010101 => CachedFish.Where(f => FishUncaught(f) && f.FirstBaitLower.Contains(BaitFilterLower) && f.TerritoryLower.Contains(ZoneFilterLower)).ToList(),
                0b010110 => CachedFish.Where(f => f.NameLower.Contains(FishFilterLower) && f.FirstBaitLower.Contains(BaitFilterLower) && f.TerritoryLower.Contains(ZoneFilterLower)).ToList(),
                0b010111 => CachedFish.Where(f => FishUncaught(f) && f.NameLower.Contains(FishFilterLower) && f.FirstBaitLower.Contains(BaitFilterLower) && f.TerritoryLower.Contains(ZoneFilterLower)).ToList(),
                0b011000 => CachedFish.Where(f => f.FishingSpotLower.Contains(SpotFilterLower) && f.TerritoryLower.Contains(ZoneFilterLower)).ToList(),
                0b011001 => CachedFish.Where(f => FishUncaught(f) && f.FishingSpotLower.Contains(SpotFilterLower) && f.TerritoryLower.Contains(ZoneFilterLower)).ToList(),
                0b011010 => CachedFish.Where(f => f.NameLower.Contains(FishFilterLower) && f.FishingSpotLower.Contains(SpotFilterLower) && f.TerritoryLower.Contains(ZoneFilterLower)).ToList(),
                0b011011 => CachedFish.Where(f => FishUncaught(f) && f.NameLower.Contains(FishFilterLower) && f.FishingSpotLower.Contains(SpotFilterLower) && f.TerritoryLower.Contains(ZoneFilterLower)).ToList(),
                0b011100 => CachedFish.Where(f => f.FirstBaitLower.Contains(BaitFilterLower) && f.FishingSpotLower.Contains(SpotFilterLower) && f.TerritoryLower.Contains(ZoneFilterLower)).ToList(),
                0b011101 => CachedFish.Where(f => FishUncaught(f) && f.FirstBaitLower.Contains(BaitFilterLower) && f.FishingSpotLower.Contains(SpotFilterLower) && f.TerritoryLower.Contains(ZoneFilterLower)).ToList(),
                0b011110 => CachedFish.Where(f => f.NameLower.Contains(FishFilterLower) && f.FirstBaitLower.Contains(BaitFilterLower) && f.FishingSpotLower.Contains(SpotFilterLower) && f.TerritoryLower.Contains(ZoneFilterLower)).ToList(),
                0b011111 => CachedFish.Where(f => FishUncaught(f) && f.NameLower.Contains(FishFilterLower) && f.FirstBaitLower.Contains(BaitFilterLower) && f.FishingSpotLower.Contains(SpotFilterLower) && f.TerritoryLower.Contains(ZoneFilterLower)).ToList(),
                0b100000 => CachedFish.Where(f => !FixedFish.Contains(f)).ToList(),
                0b100001 => CachedFish.Where(f => !FixedFish.Contains(f) && FishUncaught(f)).ToList(),
                0b100010 => CachedFish.Where(f => !FixedFish.Contains(f) && f.NameLower.Contains(FishFilterLower)).ToList(),
                0b100011 => CachedFish.Where(f => !FixedFish.Contains(f) && FishUncaught(f) && f.NameLower.Contains(FishFilterLower)).ToList(),
                0b100100 => CachedFish.Where(f => !FixedFish.Contains(f) && f.FirstBaitLower.Contains(BaitFilterLower)).ToList(),
                0b100101 => CachedFish.Where(f => !FixedFish.Contains(f) && FishUncaught(f) && f.FirstBaitLower.Contains(BaitFilterLower)).ToList(),
                0b100110 => CachedFish.Where(f => !FixedFish.Contains(f) && f.NameLower.Contains(FishFilterLower) && f.FirstBaitLower.Contains(BaitFilterLower)).ToList(),
                0b100111 => CachedFish.Where(f => !FixedFish.Contains(f) && FishUncaught(f) && f.NameLower.Contains(FishFilterLower) && f.FirstBaitLower.Contains(BaitFilterLower)).ToList(),
                0b101000 => CachedFish.Where(f => !FixedFish.Contains(f) && f.FishingSpotLower.Contains(SpotFilterLower)).ToList(),
                0b101001 => CachedFish.Where(f => !FixedFish.Contains(f) && FishUncaught(f) && f.FishingSpotLower.Contains(SpotFilterLower)).ToList(),
                0b101010 => CachedFish.Where(f => !FixedFish.Contains(f) && f.NameLower.Contains(FishFilterLower) && f.FishingSpotLower.Contains(SpotFilterLower)).ToList(),
                0b101011 => CachedFish.Where(f => !FixedFish.Contains(f) && FishUncaught(f) && f.NameLower.Contains(FishFilterLower) && f.FishingSpotLower.Contains(SpotFilterLower)).ToList(),
                0b101100 => CachedFish.Where(f => !FixedFish.Contains(f) && f.FirstBaitLower.Contains(BaitFilterLower) && f.FishingSpotLower.Contains(SpotFilterLower)).ToList(),
                0b101101 => CachedFish.Where(f => !FixedFish.Contains(f) && FishUncaught(f) && f.FirstBaitLower.Contains(BaitFilterLower) && f.FishingSpotLower.Contains(SpotFilterLower)).ToList(),
                0b101110 => CachedFish.Where(f => !FixedFish.Contains(f) && f.NameLower.Contains(FishFilterLower) && f.FirstBaitLower.Contains(BaitFilterLower) && f.FishingSpotLower.Contains(SpotFilterLower)).ToList(),
                0b101111 => CachedFish.Where(f => !FixedFish.Contains(f) && FishUncaught(f) && f.NameLower.Contains(FishFilterLower) && f.FirstBaitLower.Contains(BaitFilterLower) && f.FishingSpotLower.Contains(SpotFilterLower)).ToList(),
                0b110000 => CachedFish.Where(f => !FixedFish.Contains(f) && f.TerritoryLower.Contains(ZoneFilterLower)).ToList(),
                0b110001 => CachedFish.Where(f => !FixedFish.Contains(f) && FishUncaught(f) && f.TerritoryLower.Contains(ZoneFilterLower)).ToList(),
                0b110010 => CachedFish.Where(f => !FixedFish.Contains(f) && f.NameLower.Contains(FishFilterLower) && f.TerritoryLower.Contains(ZoneFilterLower)).ToList(),
                0b110011 => CachedFish.Where(f => !FixedFish.Contains(f) && FishUncaught(f) && f.NameLower.Contains(FishFilterLower) && f.TerritoryLower.Contains(ZoneFilterLower)).ToList(),
                0b110100 => CachedFish.Where(f => !FixedFish.Contains(f) && f.FirstBaitLower.Contains(BaitFilterLower) && f.TerritoryLower.Contains(ZoneFilterLower)).ToList(),
                0b110101 => CachedFish.Where(f => !FixedFish.Contains(f) && FishUncaught(f) && f.FirstBaitLower.Contains(BaitFilterLower) && f.TerritoryLower.Contains(ZoneFilterLower)).ToList(),
                0b110110 => CachedFish.Where(f => !FixedFish.Contains(f) && f.NameLower.Contains(FishFilterLower) && f.FirstBaitLower.Contains(BaitFilterLower) && f.TerritoryLower.Contains(ZoneFilterLower)).ToList(),
                0b110111 => CachedFish.Where(f => !FixedFish.Contains(f) && FishUncaught(f) && f.NameLower.Contains(FishFilterLower) && f.FirstBaitLower.Contains(BaitFilterLower) && f.TerritoryLower.Contains(ZoneFilterLower)).ToList(),
                0b111000 => CachedFish.Where(f => !FixedFish.Contains(f) && f.FishingSpotLower.Contains(SpotFilterLower) && f.TerritoryLower.Contains(ZoneFilterLower)).ToList(),
                0b111001 => CachedFish.Where(f => !FixedFish.Contains(f) && FishUncaught(f) && f.FishingSpotLower.Contains(SpotFilterLower) && f.TerritoryLower.Contains(ZoneFilterLower)).ToList(),
                0b111010 => CachedFish.Where(f => !FixedFish.Contains(f) && f.NameLower.Contains(FishFilterLower) && f.FishingSpotLower.Contains(SpotFilterLower) && f.TerritoryLower.Contains(ZoneFilterLower)).ToList(),
                0b111011 => CachedFish.Where(f => !FixedFish.Contains(f) && FishUncaught(f) && f.NameLower.Contains(FishFilterLower) && f.FishingSpotLower.Contains(SpotFilterLower) && f.TerritoryLower.Contains(ZoneFilterLower)).ToList(),
                0b111100 => CachedFish.Where(f => !FixedFish.Contains(f) && f.FirstBaitLower.Contains(BaitFilterLower) && f.FishingSpotLower.Contains(SpotFilterLower) && f.TerritoryLower.Contains(ZoneFilterLower)).ToList(),
                0b111101 => CachedFish.Where(f => !FixedFish.Contains(f) && FishUncaught(f) && f.FirstBaitLower.Contains(BaitFilterLower) && f.FishingSpotLower.Contains(SpotFilterLower) && f.TerritoryLower.Contains(ZoneFilterLower)).ToList(),
                0b111110 => CachedFish.Where(f => !FixedFish.Contains(f) && f.NameLower.Contains(FishFilterLower) && f.FirstBaitLower.Contains(BaitFilterLower) && f.FishingSpotLower.Contains(SpotFilterLower) && f.TerritoryLower.Contains(ZoneFilterLower)).ToList(),
                0b111111 => CachedFish.Where(f => !FixedFish.Contains(f) && FishUncaught(f) && f.NameLower.Contains(FishFilterLower) && f.FirstBaitLower.Contains(BaitFilterLower) && f.FishingSpotLower.Contains(SpotFilterLower) && f.TerritoryLower.Contains(ZoneFilterLower)).ToList(),
            // @formatter:on
            _ => throw new InvalidEnumArgumentException(),
        };
    }

    private static bool CheckFishType(ExtendedFish f)
    {
        if (f.Fish.IsBigFish)
            return GatherBuddy.Config.ShowBigFish;

        return f.Fish.IsSpearFish ? GatherBuddy.Config.ShowSpearFish : GatherBuddy.Config.ShowSmallFish;
    }

    private bool SelectFish(ExtendedFish f)
    {
        if (!Selector(f))
            return false;

        if (!GatherBuddy.Config.ShowAlwaysUp && f.Fish.FishRestrictions == FishRestrictions.None)
            return false;

        return CheckFishType(f);
    }

    private bool FishUncaught(ExtendedFish f)
        => !_fishManager.FishLog.IsUnlocked(f.Fish);

    private static string[] PreparePatches()
    {
        var patches = (Patch[])Enum.GetValues(typeof(Patch));
        var expansions = new[]
        {
            "All",
            "A Realm Reborn",
            "Heavensward",
            "Stormblood",
            "Shadowbringers",
            "Endwalker",
        };
        return expansions.Concat(patches
            .Select(PatchExtensions.ToVersionString)).ToArray();
    }

    private static Func<ExtendedFish, bool>[] PreparePatchSelectors()
    {
        var patches = (Patch[])Enum.GetValues(typeof(Patch));
        var expansions = new Func<ExtendedFish, bool>[]
        {
            _ => true,
            f => f.Fish.Patch.ToExpansion() == Patch.ARealmReborn,
            f => f.Fish.Patch.ToExpansion() == Patch.Heavensward,
            f => f.Fish.Patch.ToExpansion() == Patch.Stormblood,
            f => f.Fish.Patch.ToExpansion() == Patch.Shadowbringers,
            f => f.Fish.Patch.ToExpansion() == Patch.Endwalker,
        };
        return expansions.Concat(patches
            .Select(p => new Func<ExtendedFish, bool>(f => f.Fish.Patch == p))).ToArray();
    }

    private class UptimeComparer : IComparer<TimeInterval>
    {
        public int Compare(TimeInterval x, TimeInterval y)
            => (int)x.Compare(y);
    }
}
