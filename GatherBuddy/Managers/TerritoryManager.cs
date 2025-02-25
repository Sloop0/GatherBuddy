﻿using System.Collections.Generic;
using GatherBuddy.Game;
using GatherBuddy.Utility;

namespace GatherBuddy.Managers
{
    public class TerritoryManager
    {
        public Dictionary<uint, FFName>    Regions     { get; } = new();
        public Dictionary<uint, Territory> Territories { get; } = new();

        // Add region name if it does not exist.
        private FFName? FindOrAddRegionName(uint regionNameRowId)
        {
            if (Regions.TryGetValue(regionNameRowId, out var names))
                return names;

            names = FFName.FromPlaceName(regionNameRowId);
            if (names.AnyEmpty())
                return null;

            Regions[regionNameRowId] = names;
            return names;
        }

        public Territory? FindOrAddTerritory(Lumina.Excel.GeneratedSheets.TerritoryType t)
        {
            // Create territory if it does not exist. Otherwise add the aetheryte to its list.
            if (Territories.TryGetValue(t.RowId, out var territory))
                return territory;

            var names = FFName.FromPlaceName(t.PlaceName.Row);
            if (names.AnyEmpty())
                return null;

            territory = new Territory(t, FindOrAddRegionName(t.PlaceNameRegion.Row)!, names)
            {
                XStream    = t.Aetheryte.Value?.AetherstreamX ?? 0,
                YStream    = t.Aetheryte.Value?.AetherstreamY ?? 0,
                SizeFactor = t.Map.Value?.SizeFactor / 100.0f ?? 1.0f,
            };

            Territories.Add(t.RowId, territory);
            return territory;
        }
    }
}
