using GatherBuddy.Classes;
using GatherBuddy.Enums;

namespace GatherBuddy.Interfaces;

public interface ILocation : IMarkable, ITeleportable
{
    public uint          Id            { get; }
    public ObjectType    Type          { get; }
    public GatheringType GatheringType { get; }

    public int AetheryteDistance()
        => ClosestAetheryte?.WorldDistance(Territory.Id, IntegralXCoord, IntegralYCoord) ?? int.MaxValue;

    public bool OverwriteWithCustomInfo(Aetheryte? closestAetheryte, float xCoord, float yCoord);
    public void OverwriteWithDefault();
}
