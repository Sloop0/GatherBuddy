using System;

namespace GatherBuddy.Enums;

public enum SpearfishSpeed : byte
{
    Unknown       = 0,
    ExtremelySlow = 1,
    VerySlow      = 2,
    Slow          = 3,
    Fast          = 4,
    VeryFast      = 5,
    ExtremelyFast = 6,

    None = 255,
}

public static class SpearFishSpeedExtensions
{
    public static string ToName(this SpearfishSpeed speed)
        => speed switch
        {
            SpearfishSpeed.Unknown       => "Unknown Speed",
            SpearfishSpeed.ExtremelySlow => "Extremely Slow",
            SpearfishSpeed.VerySlow      => "Very Slow",
            SpearfishSpeed.Slow          => "Slow",
            SpearfishSpeed.Fast          => "Fast",
            SpearfishSpeed.VeryFast      => "Very Fast",
            SpearfishSpeed.ExtremelyFast => "Extremely Fast",
            SpearfishSpeed.None          => "No Speed",
            _                            => throw new ArgumentOutOfRangeException(nameof(speed), speed, null),
        };
}
