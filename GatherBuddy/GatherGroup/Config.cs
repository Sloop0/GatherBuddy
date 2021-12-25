using System.Collections.Generic;

namespace GatherBuddy.GatherGroup;

public struct TimedGroupNodeConfig
{
    public uint   NodeId;
    public uint   StartMinute;
    public uint   EndMinute;
    public uint   ItemId;
    public string Annotation;
}

public class TimedGroupConfig
{
    public string                     Name        = string.Empty;
    public string                     Description = string.Empty;
    public List<TimedGroupNodeConfig> Nodes       = new();
}
