using System.Collections.Generic;
using GatherBuddy.Structs;

namespace GatherBuddy.FishTimer;

public partial class Record
{
    public const uint MinTime = 1000;
    public const uint MaxTime = 45000;

    public HashSet<uint> SuccessfulBaits   { get; }      = new();
    public ushort        EarliestCatch     { get; set; } = ushort.MaxValue;
    public ushort        LatestCatch       { get; set; }
    public ushort        EarliestCatchChum { get; set; } = ushort.MaxValue;
    public ushort        LatestCatchChum   { get; set; }
    public bool          WithoutSnagging   { get; set; }

    public bool Update(Bait bait, ushort time, bool snagging, bool chum, long biteTime)
        => Update(bait, time, snagging, chum);

    public void Delete()
    {
        SuccessfulBaits.Clear();
        EarliestCatch     = ushort.MaxValue;
        EarliestCatchChum = ushort.MaxValue;
        LatestCatch       = 0;
        LatestCatchChum   = 0;
        WithoutSnagging   = false;
    }

    public bool Merge(Record rhs)
    {
        var ret = false;
        if (rhs.SuccessfulBaits.Count == 0)
            return ret;

        if (rhs.EarliestCatch < EarliestCatch)
        {
            EarliestCatch = rhs.EarliestCatch;
            ret           = true;
        }

        if (rhs.EarliestCatchChum < EarliestCatchChum)
        {
            EarliestCatchChum = rhs.EarliestCatchChum;
            ret               = true;
        }

        if (rhs.LatestCatch > LatestCatch)
        {
            LatestCatch = rhs.LatestCatch;
            ret         = true;
        }

        if (rhs.LatestCatchChum > LatestCatchChum)
        {
            LatestCatchChum = rhs.LatestCatchChum;
            ret             = true;
        }


        if (rhs.WithoutSnagging && !WithoutSnagging)
        {
            WithoutSnagging = rhs.WithoutSnagging;
            ret             = true;
        }

        foreach (var bait in rhs.SuccessfulBaits)
            ret |= SuccessfulBaits.Add(bait);

        return ret;
    }

    public bool Update(Bait bait, ushort time, bool snagging, bool chum)
    {
        var ret = false;

        if (bait.Id != 0)
            ret |= SuccessfulBaits.Add(bait.Id);

        if (time > MinTime && time < MaxTime)
        {
            if (chum)
            {
                if (time < EarliestCatchChum)
                {
                    ret               = true;
                    EarliestCatchChum = time;
                }

                if (time > LatestCatchChum)
                {
                    ret             = true;
                    LatestCatchChum = time;
                }
            }
            else
            {
                if (time < EarliestCatch)
                {
                    ret           = true;
                    EarliestCatch = time;
                }

                if (time > LatestCatch)
                {
                    ret         = true;
                    LatestCatch = time;
                }
            }
        }

        if (snagging || WithoutSnagging)
            return ret;

        WithoutSnagging = true;
        ret             = true;

        return ret;
    }
}
