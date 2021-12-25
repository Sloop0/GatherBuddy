using System;
using System.IO;
using System.Linq;
using Dalamud.Logging;
using GatherBuddy.Caching;

namespace GatherBuddy.FishTimer;

public static class FishManagerRecordExtensions
{
    public const string SaveFileName = "fishing_records.data";

    public static void SaveRecords(this FishManager manager)
    {
        var file = Utility.Util.ObtainSaveFile(SaveFileName);
        if (file == null)
            return;

        try
        {
            File.WriteAllLines(file.FullName, manager.FishInLog.Select(f => f.Value.Records.WriteLine(f.Key)));
        }
        catch (Exception e)
        {
            PluginLog.Error($"Could not write fishing records to file {file.FullName}:\n{e}");
        }
    }

    public static void LoadRecords(this FishManager manager)
    {
        var file = Utility.Util.ObtainSaveFile(SaveFileName);
        if (file is not { Exists: true })
        {
            manager.SaveRecords();
            return;
        }

        try
        {
            var changes = false;
            var text    = File.ReadAllLines(file.FullName);
            foreach (var line in text)
            {
                var p = Record.FromLine(line);
                if (p == null || !manager.FishInLog.TryGetValue(p.Value.Item1, out var fish))
                {
                    changes = true;
                    continue;
                }

                fish.Records = p.Value.Item2;
            }

            if (changes)
                manager.SaveRecords();
        }
        catch (Exception e)
        {
            PluginLog.Error($"Could not read fishing records from file {file.FullName}:\n{e}");
        }
    }

    public static int MergeFishRecords(this FishManager manager, FileInfo file)
    {
        if (!file.Exists)
            return -1;

        try
        {
            var oldFile = Utility.Util.ObtainSaveFile(SaveFileName);
            if (oldFile is { Exists: true })
                File.Copy(oldFile!.FullName, oldFile.FullName + ".bak", true);
        }
        catch (Exception e)
        {
            PluginLog.Error($"Could not create a backup of fishing records:\n{e}");
            return -1;
        }

        var sum = 0;
        try
        {
            var lines = File.ReadAllLines(file.FullName);
            foreach (var line in lines)
            {
                var p = Record.FromLine(line);
                if (p == null || !manager.FishInLog.TryGetValue(p.Value.Item1, out var fish))
                    continue;

                sum += fish.Records.Merge(p.Value.Item2) ? 1 : 0;
            }
        }
        catch (Exception e)
        {
            PluginLog.Error($"Could not read fishing records from file {file.FullName}:\n{e}");
            return -1;
        }

        return sum;
    }
}
