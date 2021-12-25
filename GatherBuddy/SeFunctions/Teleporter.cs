using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace GatherBuddy.SeFunctions;

public static unsafe class Teleporter
{
    public static bool IsAttuned(uint aetheryte)
    {
        if (Dalamud.ClientState.LocalPlayer == null)
            return false;

        var teleport = Telepo.Instance();
        if (teleport == null)
        {
            PluginLog.Error("Could not check attunement: Telepo is missing.");
            return false;
        }

        if (teleport->TeleportList.Size() == 0)
            teleport->UpdateAetheryteList();

        var endPtr = teleport->TeleportList.Last;
        for (var it = teleport->TeleportList.First; it != endPtr; ++it)
        {
            if (it->AetheryteId == aetheryte)
                return true;
        }

        return false;
    }

    public static bool Teleport(uint aetheryte)
    {
        if (IsAttuned(aetheryte))
        {
            Telepo.Instance()->Teleport(aetheryte, 0);
            return true;
        }

        Dalamud.Chat.PrintError("Could not teleport, not attuned.");
        return false;
    }

    // Teleport without checking for attunement. Use at own risk.
    public static void TeleportUnchecked(uint aetheryte)
    {
        Telepo.Instance()->Teleport(aetheryte, 0);
    }
}
