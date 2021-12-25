using GatherBuddy.Classes;

namespace GatherBuddy.Interfaces;

public interface IMarkable
{
    public string    Name           { get; }
    public Territory Territory      { get; }
    public int       IntegralXCoord { get; }
    public int       IntegralYCoord { get; }
    public float     XCoord         { get; }
    public float     YCoord         { get; }
}
