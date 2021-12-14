using System.Collections.Generic;

namespace GatherBuddyA.Classes;

public interface IGatherable
{
    public IEnumerable<ILocation> Locations { get; }
}
