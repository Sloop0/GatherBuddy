using System;
using GatherBuddy.Enums;
using ImGuiScene;

namespace GatherBuddy.Caching;

public static class IconId
{
    public const uint HookSet          = 1103;
    public const uint PowerfulHookSet  = 1115;
    public const uint PrecisionHookSet = 1116;
    public const uint Snagging         = 1109;
    public const uint Gigs             = 60037;
    public const uint SmallGig         = 60671;
    public const uint NormalGig        = 60672;
    public const uint LargeGig         = 60673;

    public const uint Speed1   = 61701;
    public const uint Speed2   = 61702;
    public const uint Speed3   = 61703;
    public const uint Speed4   = 61704;
    public const uint Speed5   = 61705;
    public const uint Speed6   = 61706;
    public const uint SpeedUnk = 61712;

    public static TextureWrap FromHookSet(HookSet hook)
        => hook switch
        {
            Enums.HookSet.Precise  => Icons.DefaultStorage[PrecisionHookSet],
            Enums.HookSet.Powerful => Icons.DefaultStorage[PowerfulHookSet],
            _                      => Icons.DefaultStorage[HookSet],
        };

    public static TextureWrap FromSize(SpearfishSize size)
        => size switch
        {
            SpearfishSize.Small   => Icons.DefaultStorage[SmallGig],
            SpearfishSize.Average => Icons.DefaultStorage[NormalGig],
            SpearfishSize.Large   => Icons.DefaultStorage[LargeGig],
            _                     => Icons.DefaultStorage[Gigs],
        };

    public static TextureWrap FromSpeed(SpearfishSpeed speed)
        => speed switch
        {
            SpearfishSpeed.ExtremelySlow => Icons.DefaultStorage[Speed1],
            SpearfishSpeed.VerySlow      => Icons.DefaultStorage[Speed2],
            SpearfishSpeed.Slow          => Icons.DefaultStorage[Speed3],
            SpearfishSpeed.Fast          => Icons.DefaultStorage[Speed4],
            SpearfishSpeed.VeryFast      => Icons.DefaultStorage[Speed5],
            SpearfishSpeed.ExtremelyFast => Icons.DefaultStorage[Speed6],
            _                            => Icons.DefaultStorage[SpeedUnk],
        };

    public static TextureWrap GetSnagging()
        => Icons.DefaultStorage[Snagging];
}
