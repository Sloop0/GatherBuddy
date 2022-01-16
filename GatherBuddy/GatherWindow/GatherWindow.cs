using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dalamud.Logging;
using GatherBuddy.Interfaces;
using Newtonsoft.Json;

namespace GatherBuddy.GatherWindow;

public class GatherWindow
{
    public const string FileName = "gather_window.json";

    public List<IGatherable> Items = new();

    public void AddGatherable(IGatherable item)
    {
        if (Items.Contains(item))
            return;

        Items.Add(item);
        Save();
    }

    public void RemoveLocation(IGatherable item)
    {
        if (Items.Remove(item))
            return;

        Save();
    }

    public void MoveLocation(int idx1, int idx2)
    {
        if (Utility.Functions.Swap(Items, idx1, idx2))
            Save();
    }

    public void Save()
    {
        var file = Utility.Functions.ObtainSaveFile(FileName);
        if (file == null)
            return;

        IEnumerable<(Type Type, uint Id)> data = Items.Select(i => (i.GetType(), i.ItemId));

        try
        {
            var text = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(file.FullName, text);
        }
        catch (Exception e)
        {
            PluginLog.Error($"Error serializing gather window data:\n{e}");
        }
    }

    public static GatherWindow Load()
    {
        var ret  = new GatherWindow();
        var file = Utility.Functions.ObtainSaveFile(FileName);
        if (file is not { Exists: true })
        {
            ret.Save();
            return ret;
        }

        try
        {
            var text = File.ReadAllText(file.FullName);
            var data = JsonConvert.DeserializeObject<(Type Type, uint Id)[]>(text);
            ret.Items.Capacity = data.Length;
            var change = false;
            foreach (var item in data)
            {
                // TODO
            }

            if (change)
                ret.Save();
        }
        catch (Exception e)
        {
            PluginLog.Error($"Error deserializing gather window data:\n{e}");
            ret.Items.Clear();
            ret.Save();
        }

        return ret;
    }
}
