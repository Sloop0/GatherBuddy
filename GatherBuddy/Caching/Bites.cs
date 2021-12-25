using GatherBuddy.Enums;
using GatherBuddy.Gui;

namespace GatherBuddy.Caching;

public static class Bites
{
    public static readonly (string, uint) Weak      = ("  !  ", Colors.FishTab.WeakBite);
    public static readonly (string, uint) Strong    = (" ! ! ", Colors.FishTab.StrongBite);
    public static readonly (string, uint) Legendary = (" !!! ", Colors.FishTab.LegendaryBite);
    public static readonly (string, uint) Unknown   = (" ? ? ", Colors.FishTab.UnknownBite);

    public static (string, uint) FromBiteType(BiteType bite)
        => bite switch
        {
            BiteType.Weak      => Weak,
            BiteType.Strong    => Strong,
            BiteType.Legendary => Legendary,
            _                  => Unknown,
        };
}
