using System;
using Dalamud.Data;
using System.Collections.Generic;
using System.Linq;
using GatherBuddy.Classes;
using GatherBuddy.Structs;
using Lumina.Excel.GeneratedSheets;
using Weather = GatherBuddy.Structs.Weather;

namespace GatherBuddy;

public class GameData
{
    internal DataManager                               DataManager { get; init; }
    internal Dictionary<byte, CumulativeWeatherRates>? CumulativeWeatherRates;
    internal Dictionary<uint, Territory>?              Territories = new();

    public Dictionary<uint, Weather> Weathers           { get; init; }
    public Territory[]               WeatherTerritories { get; init; }

    public Dictionary<uint, Gatherable> Gatherables { get; init; }


    public GameData(DataManager gameData)
    {
        DataManager = gameData;
        Weathers    = DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Weather>()!
            .ToDictionary(w => w.RowId, w => new Weather(w));

        CumulativeWeatherRates = DataManager.GetExcelSheet<WeatherRate>()?
            .ToDictionary(w => (byte)w.RowId, w => new CumulativeWeatherRates(this, w));

        WeatherTerritories = DataManager.GetExcelSheet<TerritoryType>()?
            .Where(t => t.WeatherRate != 0)
            .Select(FindOrAddTerritory)
            .Where(t => t != null && t.WeatherRates.Rates.Length > 1)
            .OrderBy(t => t.Name)
            .Cast<Territory>()
            .ToArray() ?? Array.Empty<Territory>();

        Gatherables = DataManager.GetExcelSheet<GatheringItem>()?
            .Where(g => g.Item != 0 && g.Item < 1000000)
            .ToDictionary(g => (uint) g.Item, g => new Gatherable(this, g))
            ?? new Dictionary<uint, Gatherable>();


        CleanTemporaries();
    }

    private void CleanTemporaries()
    {
        CumulativeWeatherRates = null;
        Territories            = null;
    }

    internal Territory? FindOrAddTerritory(TerritoryType? t)
    {
        if (Territories == null || t == null)
            return null;

        if (Territories!.TryGetValue(t.RowId, out var territory))
            return territory;

        // Create territory if it does not exist.
        territory = new Territory(this, t);
        Territories.Add(t.RowId, territory);
        return territory;
    }
}