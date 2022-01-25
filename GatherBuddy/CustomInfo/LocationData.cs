using GatherBuddy.Classes;
using GatherBuddy.Interfaces;

namespace GatherBuddy.CustomInfo;

public class LocationData
{
    public ILocation  Location;
    public Aetheryte? Aetheryte;
    public int      XCoord;
    public int      YCoord;

    public LocationData(ILocation loc)
    {
        Location  = loc;
        Aetheryte = loc.ClosestAetheryte;
        XCoord    = loc.IntegralXCoord;
        YCoord    = loc.IntegralYCoord;
    }
}
