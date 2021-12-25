using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dalamud;
using GatherBuddy.Classes;
using GatherBuddy.Enums;
using GatherBuddy.FishTimer;
using GatherBuddy.SeFunctions;
using GatherBuddy.Time;
using ImGuiScene;
using Fish = GatherBuddy.Classes.Fish;

namespace GatherBuddy.Caching;

public partial class ExtendedFish
{
    public Fish               Fish                   { get; init; }
    public Record             Records                { get; set; }
    public TextureWrap        Icon                   { get; init; }
    public string             Name                   { get; init; }
    public string             NameLower              { get; init; }
    public string             Time                   { get; init; }
    public TextureWrap[][]    WeatherIcons           { get; init; }
    public BaitOrder[]        Bait                   { get; init; }
    public string             FirstBaitLower         { get; init; }
    public string             Territory              { get; init; }
    public string             TerritoryLower         { get; init; }
    public string             FishingSpot            { get; init; }
    public (string, string)[] AdditionalFishingSpots { get; init; }
    public string             FishingSpotTcAddress   { get; init; }
    public string             FishingSpotLower       { get; init; }
    public TextureWrap?       Snagging               { get; init; }
    public Predator[]         Predators              { get; init; }
    public string             Patch                  { get; init; }
    public string             UptimeString           { get; init; }
    public string             IntuitionText          { get; init; } = string.Empty;
    public bool               HasUptimeDependency    { get; init; }
    public ushort             Uptime                 { get; init; }
    public string             FishingSpotTooltip     { get; init; }
    public string             TerritoryTooltip       { get; init; }
    public bool               IsFixed                { get; set; } = false;

    private static string SetTime(Fish fish)
    {
        if (!fish.FishRestrictions.HasFlag(FishRestrictions.Time))
            return "Always Up";

        if (fish.Interval.AlwaysUp())
            return "Unknown Uptime";

        return fish.Interval.PrintHours();
    }

    private static ushort SetUptime(Fish fish)
    {
        var uptime = 10000 * fish.Interval.OnTime / EorzeaTimeStampExtensions.MillisecondsPerEorzeaHour / RealTime.HoursPerDay;
        uptime *= fish.FishingSpots.First().Territory.WeatherRates.ChanceForWeather(fish.PreviousWeather);
        uptime *= fish.FishingSpots.First().Territory.WeatherRates.ChanceForWeather(fish.CurrentWeather);
        uptime /= 10000;
        return (ushort)uptime;
    }

    private static TextureWrap[][] SetWeather(Fish fish)
    {
        if (!fish.FishRestrictions.HasFlag(FishRestrictions.Weather))
            return Array.Empty<TextureWrap[]>();

        if (fish.PreviousWeather.Length == 0 && fish.CurrentWeather.Length == 0)
            return Array.Empty<TextureWrap[]>();

        return new[]
        {
            fish.PreviousWeather.Select(w => Icons.DefaultStorage[(uint)w.Data.Icon]).ToArray(),
            fish.CurrentWeather.Select(w => Icons.DefaultStorage[(uint)w.Data.Icon]).ToArray(),
        };
    }

    private static Predator[] SetPredators(Fish fish)
    {
        if (fish.Predators.Length == 0)
            return Array.Empty<Predator>();

        return fish.Predators.Select(p => new Predator()
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
                    Name    = $"{fish.Size} Fish Size",
                    Fish    = null,
                    Icon    = IconId.FromSize(fish.Size),
                    Bite    = Bites.Unknown,
                    HookSet = null,
                },
            };

        if (fish.InitialBait.Equals(Structs.Bait.Unknown))
            return Array.Empty<BaitOrder>();

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

    private static string GetFishingSpotTcAddress(uint fishingSpotId)
    {
        var lang = GatherBuddy.Language switch
        {
            ClientLanguage.English  => "en",
            ClientLanguage.German   => "de",
            ClientLanguage.French   => "fr",
            ClientLanguage.Japanese => "ja",
            _                       => "en",
        };
        var s = $"https://ffxivteamcraft.com/db/{lang}/fishing-spot/{fishingSpotId}";
        return s;
    }

    private static (string, string)[] SetAdditionalSpots(Fish fish)
        => fish.FishingSpots.Count > 1
            ? fish.FishingSpots.Select(s => (s.Name, s.Territory.Name)).ToArray()
            : Array.Empty<(string, string)>();

    public ExtendedFish(Fish fish)
    {
        Fish                   = fish;
        Records                = new Record();
        IsFixed                = false;
        Icon                   = Icons.DefaultStorage[fish.ItemData.Icon];
        Name                   = fish.Name[GatherBuddy.Language];
        NameLower              = Name.ToLowerInvariant();
        Time                   = SetTime(fish);
        WeatherIcons           = SetWeather(fish);
        Bait                   = SetBait(fish);
        Uptime                 = SetUptime(fish);
        UptimeString           = $"{(Uptime / 100f).ToString("F1", CultureInfo.InvariantCulture)}%%";
        FirstBaitLower         = Bait.Length > 0 ? Bait[0].Name.ToLowerInvariant() : "unknown bait";
        Predators              = SetPredators(fish);
        Snagging               = SetSnagging(fish, Bait);
        HasUptimeDependency    = SetUptimeDependency(fish, Bait);
        Patch                  = $"Patch {fish.Patch.ToVersionString()}";
        FishingSpot            = fish.FishingSpots.First().Name;
        FishingSpotTcAddress   = GetFishingSpotTcAddress(fish.FishingSpots.First().Id);
        AdditionalFishingSpots = SetAdditionalSpots(fish);
        var tmpTerritories = AdditionalFishingSpots.Select(s => s.Item2).Distinct().ToArray();
        Territory        = fish.FishingSpots.First().Territory.Name;
        TerritoryLower   = string.Join('\0', tmpTerritories.Select(s => s.ToLowerInvariant()));
        FishingSpotLower = string.Join('\0', AdditionalFishingSpots.Select(s => s.Item1.ToLowerInvariant()));

        FishingSpotTooltip = $"{Territory}\nRight-click to open TeamCraft site for this spot.";
        if (AdditionalFishingSpots.Any())
            FishingSpotTooltip +=
                $"\nAdditional Fishing Spots:\n\t\t{string.Join("\n\t\t", AdditionalFishingSpots.Select(s => $"{s.Item1} ({s.Item2})"))}";

        TerritoryTooltip = tmpTerritories.Length > 1 ? $"Additional Zones:\n\t\t{string.Join("\n\t\t", tmpTerritories)}" : string.Empty;

        var intuition = fish.IntuitionLength;
        if (intuition <= 0)
            return;

        var minutes = intuition / RealTime.SecondsPerMinute;
        var seconds = intuition % RealTime.SecondsPerMinute;
        if (seconds == 0)
            IntuitionText = minutes == 1 ? $"Intuition for {minutes} Minute" : $"Intuition for {minutes} Minutes";
        else
            IntuitionText = $"Intuition for {minutes}:{seconds:D2} Minutes";
    }
}

public partial class ExtendedFish
{
    private TimeInterval _nextUptime;

    private static TimeInterval GetUptime(Fish fish, Territory territory)
    {
        if (fish.FishRestrictions == FishRestrictions.Time)
            return fish.Interval.NextRealUptime();

        var wl      = GatherBuddy.WeatherManager.RequestForecast(territory, fish.CurrentWeather, fish.PreviousWeather, fish.Interval);
        var end     = GatherBuddy.WeatherManager.ExtendedDuration(territory, fish.CurrentWeather, fish.PreviousWeather, wl);
        var overlap = new TimeInterval(wl.Timestamp, end).FirstOverlap(fish.Interval);
        return overlap;
    }

    public TimeInterval NextUptime(Territory? territory, out bool cacheUpdated)
    {
        cacheUpdated = false;
        // Always up
        if (Fish.FishRestrictions == FishRestrictions.None)
            return TimeInterval.Always;

        // Unknown
        if (Fish.FishRestrictions.HasFlag(FishRestrictions.Time) && Fish.Interval.AlwaysUp()
         || Fish.FishRestrictions.HasFlag(FishRestrictions.Weather) && Fish.PreviousWeather.Length == 0 && Fish.CurrentWeather.Length == 0)
            return TimeInterval.Invalid;


        // If different from home territory is requested
        if (territory != null && territory.Id != Fish.FishingSpots.First().Territory.Id)
        {
            cacheUpdated = true;
            return GetUptime(Fish, territory);
        }

        // Cache valid
        if (_nextUptime.End > SeTime.ServerTime)
            return _nextUptime;

        // Update cache if necessary
        cacheUpdated = true;
        _nextUptime  = GetUptime(Fish, Fish.FishingSpots.First().Territory);
        return _nextUptime;
    }

    public TimeInterval NextUptime()
        => NextUptime(Fish.FishingSpots.First().Territory!, out _);

    public TimeInterval NextUptime(Territory? territory)
        => NextUptime(territory, out _);

    public TimeInterval NextUptime(out bool cacheUpdated)
        => NextUptime(Fish.FishingSpots.First().Territory!, out cacheUpdated);
}