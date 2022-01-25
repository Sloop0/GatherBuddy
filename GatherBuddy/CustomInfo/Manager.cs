using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Dalamud.Logging;
using GatherBuddy.Classes;
using GatherBuddy.Interfaces;
using GatherBuddy.Plugin;
using Newtonsoft.Json;

namespace GatherBuddy.CustomInfo;

public class LocationManager
{
    public readonly List<ILocation> AllLocations = GatherBuddy.GameData.GatheringNodes.Values
        .Cast<ILocation>()
        .Concat(GatherBuddy.GameData.FishingSpots.Values)
        .ToList();



    public const string FileName = "custom_locations.json";

    public List<LocationData> CustomLocations { get; private set; } = new();

    public void AddOrChangeLocation(LocationData data)
    {
        //var loc = CustomLocations.FirstOrDefault(l => ReferenceEquals(data.Location, l.Location));
        //if (loc == null)
        //{
        //    CustomLocations.Add(data);
        //    data.Location.SetAetheryte(data.Aetheryte);
        //    data.Location.SetXCoord(data.XCoord);
        //    data.Location.SetYCoord(data.YCoord);
        //    Save();
        //}
        //else 
        //{
        //    if (data.Location.IntegralXCoord != data.XCoord)
        //        data.Location.Set
        //    loc.Aetheryte = data.Aetheryte;
        //    loc.XCoord    = data.XCoord;
        //    loc.YCoord    = data.YCoord;
        //    loc.Location.OverwriteWithCustomInfo(loc.Aetheryte, loc.XCoord, loc.YCoord);
        //    Save();
        //}
    }

    public void RemoveLocation(ILocation location)
    {
        //if (CustomLocations.RemoveAll(l => ReferenceEquals(l.Location, location)) <= 0)
        //    return;
        //
        //location.OverwriteWithCustomInfo(location.DefaultAetheryte, location.DefaultXCoord / 100f, location.DefaultYCoord / 100f);
        //Save();
    }

    public void MoveLocation(int idx1, int idx2)
    {
        if (Functions.Swap(CustomLocations, idx1, idx2))
            Save();
    }

    public void Save()
    {
        //var file = Functions.ObtainSaveFile(FileName);
        //if (file == null)
        //    return;
        //
        //try
        //{
        //    IEnumerable<(Type Type, uint Id, uint AetheryteId, float X, float Y)> locations = CustomLocations.Select(l
        //        => (l.Location.GetType(), l.Location.Id, l.Location.ClosestAetheryte?.Id ?? 0, l.XCoord, l.YCoord));
        //    var text = JsonConvert.SerializeObject(locations, Formatting.Indented);
        //    File.WriteAllText(file.FullName, text);
        //}
        //catch (Exception e)
        //{
        //    PluginLog.Error($"Could not write custom locations to file {file.FullName}:\n{e}");
        //}
    }

    public static LocationManager Load()
    {
        var     file = Functions.ObtainSaveFile(FileName);
        LocationManager ret  = new();
        if (file is not { Exists: true })
        {
            ret.Save();
            return ret;
        }

        try
        {
            var changes   = false;
            var text      = File.ReadAllText(file.FullName);
            var locations = JsonConvert.DeserializeObject<(Type Type, uint Id, uint AetheryteId, float X, float Y)[]>(text);
            ret.CustomLocations.Capacity = locations.Length;
            foreach (var location in locations)
            {
                Aetheryte? aetheryte = null;
                if (location.AetheryteId != 0 && !GatherBuddy.GameData.Aetherytes.TryGetValue(location.AetheryteId, out aetheryte))
                {
                    changes = true;
                    PluginLog.Error($"Invalid aetheryte id {location.AetheryteId} in custom locations.");
                    continue;
                }

                //if (location.Type == typeof(FishingSpot) && GatherBuddy.GameData.FishingSpots.TryGetValue(location.Id, out var spot))
                //    ret.CustomLocations.Add(new LocationData(spot)
                //    {
                //        Aetheryte = aetheryte,
                //        XCoord    = location.X,
                //        YCoord    = location.Y,
                //    });
                //else if (location.Type == typeof(GatheringNode) && GatherBuddy.GameData.GatheringNodes.TryGetValue(location.Id, out var node))
                //    ret.CustomLocations.Add(new LocationData(node)
                //    {
                //        Aetheryte = aetheryte,
                //        XCoord    = location.X,
                //        YCoord    = location.Y,
                //    });
                //else
                //    changes = true;
            }

            if (changes)
                ret.Save();
        }
        catch (Exception e)
        {
            PluginLog.Error($"Error loading custom infos:\n{e}");
        }

        return ret;
    }
}
