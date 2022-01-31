using System.Collections.Generic;

namespace GatherBuddy.FishTimer;

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

    public static Dictionary<uint, FishRecordTimes> FromBites(IList<FishRecord> bites)
    {
        var ret = new Dictionary<uint, FishRecordTimes>();
        foreach (var bite in bites)
        {
            if (!bite.Flags.HasFlag(FishRecord.Effects.Valid))
                continue;

            if (!ret.TryGetValue(bite.Catch?.ItemId ?? 0, out var times))
            {
                times = new FishRecordTimes();
                ret.Add(bite.Catch?.ItemId ?? 0, times);
            }

            times.Apply(bite.Bait.Id, bite.Bite, bite.Flags.HasFlag(FishRecord.Effects.Chum));
        }

        return ret;
    }
}
