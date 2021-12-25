using GatherBuddy.Classes;

namespace GatherBuddy.Interfaces;

public interface ILocation : IMarkable, ITeleportable
{
    public int AetheryteDistance()
        => ClosestAetheryte?.WorldDistance(Territory.Id, IntegralXCoord, IntegralYCoord) ?? int.MaxValue;

    public bool OverwriteWithCustomInfo(Aetheryte? closestAetheryte, float xCoord, float yCoord);
    public void OverwriteWithDefault();
}
