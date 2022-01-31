using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dalamud.Logging;

namespace GatherBuddy.FishTimer;

public partial class FishRecorder
{
    public List<FishRecord>                  Records = new();
    public Dictionary<uint, FishRecordTimes> Times   = new();

    public FishRecorder()
    {
        FishRecordDirectory = Dalamud.PluginInterface.ConfigDirectory;
        try
        {
            Directory.CreateDirectory(FishRecordDirectory.FullName);
        }
        catch (Exception e)
        {
            PluginLog.Error($"Could not create fish record directory {FishRecordDirectory.FullName}:\n{e}");
        }

        if (Directory.Exists(FishRecordDirectory.FullName))
            ReadAllFiles();
    }

    private void AddUnchecked(FishRecord record)
    {
        Records.Add(record);
        if (record.Catch == null)
            return;

        if (!Times.TryGetValue(record.Catch.ItemId, out var times))
        {
            times                      = new FishRecordTimes();
            Times[record.Catch.ItemId] = times;
        }

        times.Apply(record.Bait.Id, record.Bite, record.Flags.HasFlag(FishRecord.Effects.Chum));
    }

    public void Add(FishRecord record)
    {
        if (!CheckSimilarity(record))
            return;

        AddUnchecked(record);
        WriteNewestFile();
    }

    private bool CheckSimilarity(FishRecord record)
        => !Records.Any(r => r.ContentId == record.ContentId && Math.Abs(r.CastStart - record.CastStart) < 1000);
}
