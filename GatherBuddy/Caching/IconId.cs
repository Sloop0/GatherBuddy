using GatherBuddy.Enums;
using ImGuiScene;

namespace GatherBuddy.Caching;

public static class IconId
{
    public const uint HookSet          = 1103;
    public const uint PowerfulHookSet  = 1115;
    public const uint PrecisionHookSet = 1116;
    public const uint Snagging         = 1109;
    public const uint Gigs             = 1121;
    public const uint SmallGig         = 60671;
    public const uint NormalGig        = 60672;
    public const uint LargeGig         = 60673;

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
            SpearfishSize.Small  => Icons.DefaultStorage[SmallGig],
            SpearfishSize.Normal => Icons.DefaultStorage[NormalGig],
            SpearfishSize.Large  => Icons.DefaultStorage[LargeGig],
            _                    => Icons.DefaultStorage[Gigs],
        };

    public static TextureWrap GetSnagging()
        => Icons.DefaultStorage[Snagging];

}
