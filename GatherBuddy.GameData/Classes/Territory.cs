using System;
using System.Collections.Generic;
using GatherBuddyA.Structs;
using GatherBuddyA.Utility;
using TerritoryType = Lumina.Excel.GeneratedSheets.TerritoryType;

namespace GatherBuddyA.Classes;

public class Territory : IComparable<Territory>
{
    public static readonly Territory Invalid = new();

    public TerritoryType          Data         { get; }       = new();
    public string                 Name         { get; }       = string.Empty;
    public HashSet<Aetheryte>     Aetherytes   { get; }       = new();
    public CumulativeWeatherRates WeatherRates { get; init; } = CumulativeWeatherRates.StaticWeather;
    public float                  SizeFactor   { get; init; }
    public int                    XStream      { get; init; }
    public int                    YStream      { get; init; }


    public uint Id
        => Data.RowId;

    public Territory(GameData gameData, TerritoryType data)
    {
        Data       = data;
        Name       = MultiString.ParseSeStringLumina(data.PlaceName.Value?.Name);
        SizeFactor = (data.Map.Value?.SizeFactor ?? 100f) / 100f;
        var aetheryte = data.Aetheryte.Value;
        if (aetheryte != null)
        {
            XStream = aetheryte.AetherstreamX;
            YStream = aetheryte.AetherstreamY;
        }

        WeatherRates = (gameData.CumulativeWeatherRates?.TryGetValue(data.WeatherRate, out var wr) ?? false) && wr.Rates.Length > 1
            ? wr
            : CumulativeWeatherRates.StaticWeather;
    }

    private Territory()
    { }

    public override string ToString()
        => Name;

    public int CompareTo(Territory? other)
        => Id.CompareTo(other?.Id ?? 0);
}
