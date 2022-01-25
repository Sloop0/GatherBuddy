using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Logging;
using GatherBuddy.Classes;
using GatherBuddy.Enums;
using GatherBuddy.Structs;
using GatherBuddy.Time;

namespace GatherBuddy.FishTimer.Parser;

public struct BiteRecord
{
    [Flags]
    public enum Effects : ushort
    {
        Snagging      = 0x01,
        Chum          = 0x02,
        Intuition     = 0x04,
        FishEyes      = 0x08,
        IdenticalCast = 0x10,

        Valid = 0x8000,
    }

    public ulong     ContentId;
    public TimeStamp CastStart;
    public uint      BaitId;
    public uint      CatchId;
    public ushort    Bite;
    public ushort    FishingSpotId;
    public Effects   Flags;
    public BiteType  BiteType;
}


public class FishRecordTimes
{
    public struct Times
    {
        public ushort Min = ushort.MaxValue;
        public ushort Max;
        public ushort MinChum = ushort.MaxValue;
        public ushort MaxChum;

        public bool Apply(ushort duration, bool chum)
        {
            var ret = false;
            if (chum)
            {
                if (duration > MaxChum)
                {
                    MaxChum = duration;
                    ret     = true;
                }
            }
            else
            {
                if (duration < Min)
                {
                    Min = duration;
                    ret = true;
                }
            }

            if (duration > Max)
            {
                Max = duration;
                ret = true;
            }

            if (duration < MinChum)
            {
                MinChum = duration;
                ret     = true;
            }

            return ret;
        }
    }

    public Times                   All;
    public SortedList<uint, Times> Data = new();

    public bool Apply(uint baitId, ushort duration, bool chum)
    {
        var ret       = All.Apply(duration, chum);
        var baitTimes = Data.TryGetValue(baitId, out var b) ? b : new Times();
        ret          |= baitTimes.Apply(duration, chum);
        Data[baitId] =  baitTimes;
        return ret;
    }

    public static bool FromBites(IList<BiteRecord> bites, out Dictionary<uint, FishRecordTimes> ret)
    {
        ret = new Dictionary<uint, FishRecordTimes>();
        var changes = false;
        for (var i = 0; i < bites.Count; ++i)
        {
            var bite = bites[i];
            if (!bite.Flags.HasFlag(BiteRecord.Effects.Valid))
                continue;

            var duration = bite.Bite - bite.CastStart;
            if (duration is < 0 or > ushort.MaxValue)
            {
                bites[i] = bite with { Flags = bite.Flags & ~BiteRecord.Effects.Valid };
                changes  = true;
                continue;
            }

            if (!ret.TryGetValue(bite.CatchId, out var times))
            {
                times = new FishRecordTimes();
                ret.Add(bite.CatchId, times);
            }

            times.Apply(bite.BaitId, (ushort)duration, bite.Flags.HasFlag(BiteRecord.Effects.Chum));
        }

        return changes;
    }
}

public class FishingRecords
{
    private DataTable _table;


    public FishingRecords()
    {
    }
}
