using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dalamud.Logging;
using GatherBuddy.Interfaces;
using Newtonsoft.Json;

namespace GatherBuddy.CustomInfo;

public class Manager
{
    public const string FileName = "custom_locations.json";

    public List<LocationData> CustomLocations { get; private set; } = new();

    public void AddOrChangeLocation(LocationData data)
    {
        var loc = CustomLocations.FirstOrDefault(l => ReferenceEquals(data.Location, l.Location));
        if (loc == null)
        {
            CustomLocations.Add(data);
            data.Location.OverwriteWithCustomInfo(data.Aetheryte, data.XCoord, data.YCoord);
            Save();
        }
        else if (loc.XCoord != data.XCoord || loc.YCoord != data.YCoord || !ReferenceEquals(loc.Aetheryte, data.Aetheryte))
        {
            loc.Aetheryte = data.Aetheryte;
            loc.XCoord    = data.XCoord;
            loc.YCoord    = data.YCoord;
            loc.Location.OverwriteWithCustomInfo(loc.Aetheryte, loc.XCoord, loc.YCoord);
            Save();
        }
    }

    public void RemoveLocation(ILocation location)
    {
        if (CustomLocations.RemoveAll(l => ReferenceEquals(l.Location, location)) <= 0)
            return;

        location.OverwriteWithDefault();
        Save();
    }

    public void Save()
    {
        var file = Utility.Util.ObtainSaveFile(FileName);
        if (file == null)
            return;

        try
        {
            File.WriteAllText(file.FullName, JsonConvert.SerializeObject(CustomLocations, Formatting.Indented));
        }
        catch (Exception e)
        {
            PluginLog.Error($"Could not write custom locations to file {file.FullName}:\n{e}");
        }
    }

    public static Manager Load()
    {
        var     file = Utility.Util.ObtainSaveFile(FileName);
        Manager ret  = new();
        if (file is not { Exists: true })
        {
            ret.Save();
            return ret;
        }

        try
        {
            var changes = false;
            var text    = File.ReadAllText(file.FullName);
            var locations = JsonConvert.DeserializeObject<List<LocationData>>(text, new JsonSerializerSettings()
            {
                Error = (_, args) =>
                {
                    changes                   = true;
                    args.ErrorContext.Handled = true;
                },
            });
            ret.CustomLocations = locations ?? new List<LocationData>();
            if (changes || locations == null)
                ret.Save();
        }
        catch (Exception e)
        {
            PluginLog.Error($"Error loading custom infos:\n{e}");
        }

        return ret;
    }
}
