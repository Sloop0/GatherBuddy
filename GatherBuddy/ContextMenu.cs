using System;
using Dalamud.Game.Gui.ContextMenus;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;

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

    private void AddEntry(ContextMenuOpenedArgs args, uint id)
    {
        if (id > 500000)
            id -= 500000;

        if (GatherBuddy.GameData.Gatherables.TryGetValue(id, out var gatherable))
            args.AddCustomItem("Gather", _ => _executor.GatherItem(gatherable));
        else if (GatherBuddy.GameData.Fishes.TryGetValue(id, out var fish))
            args.AddCustomItem("Gather", _ => _executor.GatherItem(fish));
    }

    private unsafe void AddMenuItem(ContextMenuOpenedArgs args)
    {
        if (args.InventoryItemContext != null)
        {
            AddEntry(args, args.InventoryItemContext.Id);
        }
        else if (args.ParentAddonName != null)
        {
            if (args.ParentAddonName == "ContentsInfoDetail")
            {
                var agent  = Dalamud.GameGui.FindAgentInterface("ContentsInfo");
                var itemId = *(uint*)(agent + 0x1764);
                AddEntry(args, itemId);
            }
            else
            {
                var agent = Dalamud.GameGui.FindAgentInterface(args.ParentAddonName);
                if (agent == null)
                    return;

                var itemId = args.ParentAddonName switch
                {
                    "RecipeNote"    => *(uint*)(agent + 0x398),
                    "GatheringNote" => *(uint*)(agent + 0xA0),
                    "ItemSearch"    => *(uint*)((byte*)args.Agent + 0x1738),
                    _               => 0u,
                };

                if (itemId != 0)
                    AddEntry(args, itemId);
            }
        }
    }
}
