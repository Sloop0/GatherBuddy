using ImGuiScene;

namespace GatherBuddy.Caching;

internal struct BaitOrder
{
    public TextureWrap    Icon;
    public string         Name;
    public object?        Fish;
    public TextureWrap?   HookSet;
    public (string, uint) Bite;
}
