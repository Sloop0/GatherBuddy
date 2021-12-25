using System.Linq;
using GatherBuddy.Classes;
using GatherBuddy.Enums;
using GatherBuddy.SeFunctions;
using GatherBuddy.Time;

namespace GatherBuddy.Caching;

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
