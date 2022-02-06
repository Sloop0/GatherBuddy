﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Dalamud.Logging;

namespace GatherBuddy.FishTimer;

public partial class FishRecorder : IDisposable
{
    public readonly List<FishRecord>                  Records = new();
    public readonly Dictionary<uint, FishRecordTimes> Times   = new();

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

        SubscribeToParser();
    }

    public void Enable()
    {
        Parser.Enable();
        Dalamud.Framework.Update += OnFrameworkUpdate;
    }

    public void Disable()
    {
        Dalamud.Framework.Update -= OnFrameworkUpdate;
        Parser.Disable();
    }

    public void Dispose()
        => Disable();

    private void AddUnchecked(FishRecord record)
    {
        Records.Add(record);
        AddRecordToTimes(record);
    }

    private void AddRecordToTimes(FishRecord record)
    {
        if (record.Catch == null || !record.Flags.HasFlag(FishRecord.Effects.Valid))
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

    public void Remove(int idx)
    {
        Debug.Assert(idx >= 0 && idx < Records.Count);
        var record = Records[idx];
        Records.RemoveAt(idx);
        WriteAllFiles(idx);
        RemoveRecordFromTimes(record);
    }

    private void RemoveRecordFromTimes(FishRecord record)
    {
        if (!record.Flags.HasFlag(FishRecord.Effects.Valid) || !record.HasCatch)
            return;

        if (!Times.TryGetValue(record.Catch!.ItemId, out var data) || !data.Data.TryGetValue(record.Bait.Id, out var times))
        {
            PluginLog.Error("Invalid state in fish records.");
            return;
        }

        if (times.Max != record.Bite && times.MaxChum != record.Bite && times.Min != record.Bite && times.MinChum != record.Bite)
            return;

        data.Data.Remove(record.Bait.Id);
        foreach (var rec in Records.Where(r
                     => r.Flags.HasFlag(FishRecord.Effects.Valid) && r.Catch?.ItemId == record.Catch.ItemId && r.Bait.Id == record.Bait.Id))
            data.Apply(rec.Bait.Id, rec.Bite, rec.Flags.HasFlag(FishRecord.Effects.Chum));

        if (data.Data.Count != 0)
        {
            if (data.All.Max == record.Bite)
                data.All.Max = data.Data.Values.Max(r => r.Max);
            if (data.All.MaxChum == record.Bite)
                data.All.MaxChum = data.Data.Values.Max(r => r.MaxChum);
            if (data.All.Min == record.Bite)
                data.All.Min = data.Data.Values.Min(r => r.Min);
            if (data.All.MinChum == record.Bite)
                data.All.MinChum = data.Data.Values.Min(r => r.MinChum);
        }
        else
        {
            data.All = default;
        }
    }

    private void ResetTimes()
    {
        Times.Clear();
        foreach (var record in Records)
            AddRecordToTimes(record);
    }

    public void RemoveInvalid()
    {
        if (Records.RemoveAll(r => !r.Flags.HasFlag(FishRecord.Effects.Valid)) <= 0)
            return;

        WriteAllFiles();
        ResetTimes();
    }

    public void RemoveDuplicates()
    {
        var oldCount = Records.Count;
        for (var i = 0; i < Records.Count; ++i)
        {
            var rec = Records[i];
            for (var j = Records.Count - 1; j > i; --j)
            {
                if (Similar(rec, Records[j]))
                    Records.RemoveAt(j);
            }
        }

        if (oldCount == Records.Count)
            return;

        WriteAllFiles();
        ResetTimes();
    }

    private static bool Similar(FishRecord lhs, FishRecord rhs)
        => lhs.ContentIdHash == rhs.ContentIdHash && Math.Abs(lhs.TimeStamp - rhs.TimeStamp) < 1000;

    private bool CheckSimilarity(FishRecord record)
        => !Records.Any(r => Similar(r, record));
}
