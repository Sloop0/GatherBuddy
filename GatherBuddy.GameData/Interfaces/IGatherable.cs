using System.Collections.Generic;
using GatherBuddy.Utility;

namespace GatherBuddy.Interfaces;

public interface IGatherable
{
    public MultiString            Name      { get; }
    public IEnumerable<ILocation> Locations { get; }
}
