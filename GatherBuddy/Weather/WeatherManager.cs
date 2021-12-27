using System;
using System.Collections.Generic;
using System.Linq;
using GatherBuddy.Classes;
using GatherBuddy.SeFunctions;
using GatherBuddy.Time;

namespace GatherBuddy.Weather;

public partial class WeatherManager
{
    public Dictionary<Territory, WeatherTimeline> Forecast    { get; }
    public List<WeatherTimeline>                  UniqueZones { get; } = new();


    public WeatherManager()
    {
        Forecast = GatherBuddy.GameData.WeatherTerritories.ToDictionary(t => t, t => new WeatherTimeline(t));
        foreach (var t in Forecast.Values.Where(t => UniqueZones.All(l => l.Territory.Name != t.Territory.Name)))
            UniqueZones.Add(t);
    }

    public static DateTime[] NextWeatherChangeTimes(int num, long offset = 0)
    {
        var currentWeatherTime = (SeTime.ServerTime + offset).SyncToEorzeaWeather();
        var ret                = new DateTime[num];
        for (var i = 0; i < num; ++i)
            ret[i] = (currentWeatherTime + i * EorzeaTimeStampExtensions.MillisecondsPerEorzeaWeather).LocalTime;
        return ret;
    }

    private WeatherTimeline FindOrCreateForecast(Territory territory, uint increment)
    {
        if (Forecast.TryGetValue(territory, out var values))
            return values;

        var timeline = new WeatherTimeline(territory, increment);
        Forecast[territory] = timeline;
        return timeline;
    }

    public WeatherTimeline RequestForecast(Territory territory, uint amount)
    {
        var list = FindOrCreateForecast(territory, amount);
        return list.Update(amount);
    }

    public WeatherListing RequestForecast(Territory territory, IList<Structs.Weather> weather, long offset = 0, uint increment = 32)
        => RequestForecast(territory, weather, Array.Empty<Structs.Weather>(), RepeatingInterval.Always, offset, increment);

    public WeatherListing RequestForecast(Territory territory, IList<Structs.Weather> weather, RepeatingInterval eorzeanHours, long offset = 0,
        uint increment = 32)
        => RequestForecast(territory, weather, Array.Empty<Structs.Weather>(), eorzeanHours, offset, increment);


    public WeatherListing RequestForecast(Territory territory, IList<Structs.Weather> weather, IList<Structs.Weather> previousWeather,
        RepeatingInterval eorzeanHours, long offset = 0, uint increment = 32)
    {
        var values = FindOrCreateForecast(territory, increment);
        return values.Find(weather, previousWeather, eorzeanHours, offset, increment);
    }

    public TimeStamp ExtendedDuration(Territory territory, IList<Structs.Weather> weather, IList<Structs.Weather> previousWeather,
        WeatherListing listing,
        uint increment = 32)
    {
        var checkWeathers = weather.Any();
        var checkPrevious = previousWeather.Any();
        if (!checkWeathers && !checkPrevious)
            return TimeStamp.MaxValue;

        var duration = listing.End;
        if (checkPrevious && previousWeather.All(w => w.Id != listing.Weather.Id))
            return duration;

        var values = FindOrCreateForecast(territory, increment);
        values.TrimFront();
        var idx = values.FindIndex(listing);
        if (idx < 0)
            return duration;

        for (var sanityStop = 0; sanityStop < 24; ++sanityStop)
        {
            if (checkPrevious && previousWeather.All(w => w.Id != listing.Weather.Id))
                return duration;

            if (idx == values.List.Count - 1)
                values.Append(increment);
            listing = values.List[++idx];
            if (checkWeathers && weather.All(w => w.Id != listing.Weather.Id))
                return duration;

            duration += EorzeaTimeStampExtensions.MillisecondsPerEorzeaWeather;
        }

        return duration;
    }
}
