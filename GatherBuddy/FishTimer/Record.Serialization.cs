using System.Text;
using Dalamud.Logging;

namespace GatherBuddy.FishTimer;

public partial class Record
{
    public string WriteLine(uint fishId)
    {
        var sb = new StringBuilder(128);
        sb.Append(fishId.ToString());
        sb.Append(" : { ");
        sb.Append(EarliestCatch.ToString());
        sb.Append(" ");
        sb.Append(LatestCatch.ToString());
        sb.Append(" ");
        sb.Append(EarliestCatchChum.ToString());
        sb.Append(" ");
        sb.Append(LatestCatchChum.ToString());
        sb.Append(WithoutSnagging ? " false [ " : " true [ ");
        foreach (var bait in SuccessfulBaits)
        {
            sb.Append(bait.ToString());
            sb.Append(" ");
        }

        sb.Append("]}");
        return sb.ToString();
    }



    public static (uint, Record)? FromLine(string line)
    {
        (uint, Record)? Error()
        {
            PluginLog.Error($"Could not create fishing record from \"{line}\".");
            return null;
        }

        line = MigrateToV3(line);
        var split  = line.Split(' ');
        var length = split.Length;

        const int fishIdOffset         = 0;
        const int colonOffset          = 1;
        const int openBraceOffset      = 2;
        var       earlyCatchOffset     = 3;
        var       lateCatchOffset      = 4;
        var       earlyCatchChumOffset = 5;
        var       lateCatchChumOffset  = 6;
        var       snaggingOffset       = 7;
        var       openBracketOffset    = 8;
        var       firstBaitOffset      = 9;
        var       closeBracketOffset   = length - 1;

        if (length < 8)
            return Error();

        if (length < 9 || split[openBracketOffset] != "[")
            if (split[openBracketOffset - 2] == "[")
            {
                earlyCatchChumOffset =  -1;
                lateCatchChumOffset  =  -1;
                snaggingOffset       -= 2;
                openBracketOffset    -= 2;
                firstBaitOffset      -= 2;
            }

        if (split[colonOffset] != ":"
         || split[openBraceOffset] != "{"
         || split[openBracketOffset] != "["
         || split[closeBracketOffset] != "]}")
            return Error();

        if (!uint.TryParse(split[fishIdOffset], out var fishId))
            return Error();

        if (!ushort.TryParse(split[earlyCatchOffset], out var earliestCatch))
            return Error();

        if (!ushort.TryParse(split[lateCatchOffset], out var latestCatch))
            return Error();

        ushort earliestCatchChum;
        if (earlyCatchChumOffset == -1)
            earliestCatchChum = ushort.MaxValue;
        else if (!ushort.TryParse(split[earlyCatchChumOffset], out earliestCatchChum))
            return Error();

        ushort latestCatchChum;
        if (lateCatchChumOffset == -1)
            latestCatchChum = 0;
        else if (!ushort.TryParse(split[lateCatchChumOffset], out latestCatchChum))
            return Error();

        if (!bool.TryParse(split[snaggingOffset], out var snagging))
            return Error();


        Record ret = new()
        {
            EarliestCatch     = earliestCatch,
            EarliestCatchChum = earliestCatchChum,
            LatestCatch       = latestCatch,
            LatestCatchChum   = latestCatchChum,
            WithoutSnagging   = !snagging,
        };
        for (var i = firstBaitOffset; i < closeBracketOffset; ++i)
        {
            if (!uint.TryParse(split[i], out var baitId))
                return Error();

            ret.SuccessfulBaits.Add(baitId);
        }

        return (fishId, ret);
    }
}
