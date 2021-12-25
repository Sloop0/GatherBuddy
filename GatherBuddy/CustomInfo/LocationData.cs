using GatherBuddy.Classes;
using GatherBuddy.Interfaces;
using Newtonsoft.Json;

namespace GatherBuddy.CustomInfo;

[JsonConverter(typeof(LocationDataConverter))]
public class LocationData
{
    public ILocation  Location;
    public Aetheryte? Aetheryte;
    public float      XCoord;
    public float      YCoord;

    public LocationData(ILocation loc)
    {
        Location  = loc;
        Aetheryte = loc.ClosestAetheryte;
        XCoord    = loc.IntegralXCoord / 100f;
        YCoord    = loc.IntegralYCoord / 100f;
    }
}
