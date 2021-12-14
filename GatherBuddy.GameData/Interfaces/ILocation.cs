using GatherBuddyA.Utility;

namespace GatherBuddyA.Classes;

public interface ILocation : IMarkable, ITeleportable
{
    public int AetheryteDistance()
        => ClosestAetheryte?.WorldDistance(Territory.Id, IntegralXCoord, IntegralYCoord) ?? int.MaxValue;
}