namespace GatherBuddy.Classes;

internal class DefaultInfo
{
    public Aetheryte? ClosestAetheryte;
    public int        IntegralXCoord;
    public int        IntegralYCoord;
}

public partial class GatheringNode
{
    public Territory  Territory        { get; init; }
    public Aetheryte? ClosestAetheryte { get; internal set; }

    public int IntegralXCoord { get; internal set; }
    public int IntegralYCoord { get; internal set; }

    public float XCoord
        => IntegralXCoord / 100f;

    public float YCoord
        => IntegralYCoord / 100f;

    private DefaultInfo? _defaultInfo;

    public bool OverwriteWithCustomInfo(Aetheryte? closestAetheryte, float xCoord, float yCoord)
    {
        if (closestAetheryte is { Id: 0 })
            return false;
        if (xCoord is < 1f or > 42f)
            return false;
        if (yCoord is < 1f or > 42f)
            return false;

        _defaultInfo ??= new DefaultInfo()
        {
            ClosestAetheryte = ClosestAetheryte,
            IntegralXCoord   = IntegralXCoord,
            IntegralYCoord   = IntegralYCoord,
        };

        ClosestAetheryte = closestAetheryte;
        IntegralXCoord   = (int)(100f * xCoord);
        IntegralYCoord   = (int)(100f * yCoord);
        return true;
    }

    public void OverwriteWithDefault()
    {
        if (_defaultInfo == null)
            return;

        ClosestAetheryte = _defaultInfo.ClosestAetheryte;
        IntegralXCoord   = _defaultInfo.IntegralXCoord;
        IntegralYCoord   = _defaultInfo.IntegralYCoord;
    }
}
