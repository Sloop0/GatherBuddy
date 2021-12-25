using System.Collections.Generic;
using System.Linq;
using Dalamud.Logging;
using GatherBuddy.Enums;
using GatherBuddy.FishTimer;
using GatherBuddy.SeFunctions;

namespace GatherBuddy.Caching;

public class FishManager
{
    internal IReadOnlyDictionary<uint, ExtendedFish> FishInLog { get; init; }
    internal ExtendedFish[]                          TimedFish { get; init; }

    public FishLog FishLog { get; init; }

    public FishManager()
    {
        FishInLog = GatherBuddy.GameData.Fishes.Where(f => f.Value.InLog).ToDictionary(kvp => kvp.Key, kvp => new ExtendedFish(kvp.Value));
        var changes = false;
        for (var i = 0; i < GatherBuddy.Config.FixedFish.Count; ++i)
        {
            var id = GatherBuddy.Config.FixedFish[i];
            if (!FishInLog.TryGetValue(id, out var f))
            {
                PluginLog.Error($"Fixed Fish {id} not found and removed from config.");
                GatherBuddy.Config.FixedFish.RemoveAt(i--);
                changes = true;
            }
            else
            {
                f.IsFixed = true;
            }
        }

        if (changes)
            GatherBuddy.Config.Save();
        this.LoadRecords();

        TimedFish = FishInLog.Values.Where(f => f.Fish.FishRestrictions != FishRestrictions.None).ToArray();
        FishLog   = new FishLog();
    }
}
