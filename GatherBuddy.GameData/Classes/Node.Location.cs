using System.Collections.Generic;

namespace GatherBuddy.Classes;

public partial class GatheringNode
{
    public Territory  Territory        { get; init; }
    public Aetheryte? ClosestAetheryte { get; set; }

    public int IntegralXCoord { get; init; }
    public int IntegralYCoord { get; init; }

    public double XCoord
        => IntegralXCoord / 100.0;

    public double YCoord
        => IntegralYCoord / 100.0;
}
