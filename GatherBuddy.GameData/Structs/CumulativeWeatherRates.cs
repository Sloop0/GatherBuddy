using System;
using System.Linq;
using Dalamud.Logging;
using Lumina.Excel.GeneratedSheets;

namespace GatherBuddyA.Structs;

public readonly struct CumulativeWeatherRates
{
    public static readonly CumulativeWeatherRates StaticWeather = new(false);

    public readonly (Weather Weather, byte CumulativeRate)[] Rates;

    public CumulativeWeatherRates(GameData data, WeatherRate rate)
    {
        Rates = rate.UnkData0.Where(w => w.Rate > 0)
            .Select(w => data.Weathers.TryGetValue((uint)w.Weather, out var weather)
                ? (weather, w.Rate)
                : (Weather.Invalid, w.Rate))
            .ToArray();
        byte lastRate = 0;
        for (var i = 0; i < Rates.Length; ++i)
        {
            if (Rates[i].Weather.Id == Weather.Invalid.Id)
                PluginLog.Error("Invalid Weather requested.");
            Rates[i].CumulativeRate += lastRate;
            lastRate                =  Rates[i].CumulativeRate;
        }
    }

    private CumulativeWeatherRates(bool _)
        => Rates = Array.Empty<(Weather, byte)>();
}
