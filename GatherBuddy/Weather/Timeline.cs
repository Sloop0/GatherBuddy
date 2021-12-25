using System;
using System.Collections.Generic;
using System.Linq;
using GatherBuddy.Classes;
using GatherBuddy.SeFunctions;
using GatherBuddy.Time;

namespace GatherBuddy.Weather;

public class Timeline : IComparable<Timeline>
{
    public const int MillisecondsPerWeather = EorzeaTimeStampExtensions.MillisecondsPerEorzeaWeather;

    public Territory            Territory { get; }
    public List<WeatherListing> List      { get; }

    public WeatherListing CurrentWeather
        => Get(1);

    public WeatherListing LastWeather
        => Get(0);

    private WeatherListing Get(uint idx)
    {
        TrimFront();
        if (List.Count <= idx)
            Append((uint)(idx + 1 - List.Count));
        return List[(int)idx];
    }

    public void TrimFront()
    {
        var now    = SeTime.ServerTime;
        var remove = List.FindIndex(w => w.Offset(now) < 2 * MillisecondsPerWeather);
        if (remove > 0)
            List.RemoveRange(0, remove);
    }

    private IEnumerable<WeatherListing> RequestData(uint amount, long millisecondOffset)
        => Manager.GetForecastOffset(Territory, amount, millisecondOffset);

    public void Append(uint amount)
    {
        var offset = List.Count > 0 ? MillisecondsPerWeather - (int)List.Last().Offset(SeTime.ServerTime) : -MillisecondsPerWeather;
        List.AddRange(RequestData(amount, offset));
    }

    public Timeline Update(uint amount)
    {
        TrimFront();
        if (List.Count < amount)
            Append((uint)(amount - List.Count));
        return this;
    }

    public Timeline(Territory territory, uint cache = 32)
    {
        Territory = territory;
        List      = RequestData(cache, -MillisecondsPerWeather).ToList();
    }

    public int CompareTo(Timeline? other)
        => Territory.Id.CompareTo(other?.Territory.Id ?? 0);


    public WeatherListing Find(IList<Structs.Weather> weather, IList<Structs.Weather> previousWeather, RepeatingInterval eorzeanHours, long offset = 0,
        uint increment = 32)
    {
        var now = SeTime.ServerTime + offset;
        TrimFront();
        var previousFit = false;
        var idx         = 1;

        while (true)
        {
            for (--idx; idx < List.Count; ++idx)
            {
                if (previousFit)
                {
                    var w = List[idx];
                    if (weather.Count == 0 || weather.Contains(w.Weather))
                    {
                        var overlap  = w.Uptime.FirstOverlap(eorzeanHours);
                        var duration = overlap.Duration;
                        if (duration > 0 && overlap.End > now)
                            return w;
                    }
                }

                previousFit = previousWeather.Count == 0 || previousWeather.Contains(List[idx].Weather);
            }

            Append(increment);
        }
    }

    public int FindIndex(WeatherListing listing)
        => List.FindIndex(w => w.Timestamp == listing.Timestamp);
}
