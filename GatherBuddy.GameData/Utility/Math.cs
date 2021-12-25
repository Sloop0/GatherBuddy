namespace GatherBuddy.Utility;

public static class Math
{
    public static int SquaredDistance(int x1, int y1, int x2, int y2)
    {
        x1 -= x2;
        y1 -= y2;
        return x1 * x1 + y1 * y1;
    }
}
