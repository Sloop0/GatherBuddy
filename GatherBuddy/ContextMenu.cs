using System;
using Dalamud.Game.Gui.ContextMenus;

namespace GatherBuddy;

public class ContextMenu : IDisposable
{
    private readonly Executor _executor;

    public ContextMenu(Executor executor)
    {
        _executor = executor;
        if (GatherBuddy.Config.AddIngameContextMenus)
            Enable();
    }

    public void Enable()
        => Dalamud.ContextMenu.ContextMenuOpened += AddMenuItem;

    public void Disable()
        => Dalamud.ContextMenu.ContextMenuOpened -= AddMenuItem;

    public void Dispose()
        => Disable();

    private unsafe void AddMenuItem(ContextMenuOpenedArgs args)
    {
        if (args.InventoryItemContext != null)
        {
            var id = args.InventoryItemContext.Id;
            if (GatherBuddy.GameData.Gatherables.TryGetValue(id, out var gatherable))
                args.AddCustomItem("Gather", _ => _executor.GatherItem(gatherable));
            else if (GatherBuddy.GameData.Fishes.TryGetValue(id, out var fish))
                args.AddCustomItem("Gather", _ => _executor.GatherItem(fish));
        }
    }
}
