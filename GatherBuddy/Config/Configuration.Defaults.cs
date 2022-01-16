namespace GatherBuddy.Config;

public partial class Configuration
{
    public const string DefaultIdentifiedItemFormat = "Identified [{Id}: {Name}] for \"{Input}\".";
    public const string DefaultIdentifiedFishFormat = "Identified [{Id}: {Name}] for \"{Input}\".";
    public const string DefaultNodeAlarmFormat = "[GatherBuddy][Alarm {Name}]: The gathering node for {AllItems} {DelayString} at {Location}.";
    public const string DefaultFishAlarmFormat = "[GatherBuddy][Alarm {Name}]: The fish {FishName} at {FishingSpotName} {DelayString}. Catch with {BaitName}.";
}
