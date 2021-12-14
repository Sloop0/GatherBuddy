using Dalamud.Game.Command;
using GatherBuddy.Enums;
using GatherBuddy.Utility;
using System.IO;
using System.Linq;
using Dalamud.Logging;
using GatherBuddy.Managers;
using ImGuiNET;
using Newtonsoft.Json;

namespace GatherBuddy;

public partial class GatherBuddy
{
    private void InitializeCommands()
    {
        Dalamud.Commands.AddHandler("/gatherbuddy", new CommandInfo(OnGatherBuddy)
        {
            HelpMessage = "Use to open the GatherBuddy interface.",
            ShowInHelp  = true,
        });

        Dalamud.Commands.AddHandler("/gather", new CommandInfo(OnGather)
        {
            HelpMessage = "Mark the nearest node containing the item supplied, teleport to the nearest aetheryte, equip appropriate gear.",
            ShowInHelp  = true,
        });

        Dalamud.Commands.AddHandler("/gatherbtn", new CommandInfo(OnGatherBtn)
        {
            HelpMessage =
                "Mark the nearest botanist node containing the item supplied, teleport to the nearest aetheryte, equip appropriate gear.",
            ShowInHelp = true,
        });

        Dalamud.Commands.AddHandler("/gathermin", new CommandInfo(OnGatherMin)
        {
            HelpMessage =
                "Mark the nearest miner node containing the item supplied, teleport to the nearest aetheryte, equip appropriate gear.",
            ShowInHelp = true,
        });

        Dalamud.Commands.AddHandler("/gatherfish", new CommandInfo(OnGatherFish)
        {
            HelpMessage =
                "Mark the nearest fishing spot containing the fish supplied, teleport to the nearest aetheryte and equip fishing gear.",
            ShowInHelp = true,
        });

        Dalamud.Commands.AddHandler("/gathergroup", new CommandInfo(OnGatherGroup)
        {
            HelpMessage = "Teleport to the node of a group corresponding to current time. Use /gathergroup for more details.",
            ShowInHelp  = true,
        });

        Dalamud.Commands.AddHandler("/gatherdebug", new CommandInfo(OnGatherDebug)
        {
            HelpMessage = "Dump some collected information.",
            ShowInHelp  = false,
        });
    }

    private static void DisposeCommands()
    {
        Dalamud.Commands.RemoveHandler("/gatherdebug");
        Dalamud.Commands.RemoveHandler("/gather");
        Dalamud.Commands.RemoveHandler("/gatherbtn");
        Dalamud.Commands.RemoveHandler("/gathermin");
        Dalamud.Commands.RemoveHandler("/gatherfish");
        Dalamud.Commands.RemoveHandler("/gathergroup");
        Dalamud.Commands.RemoveHandler("/gatherbuddy");
    }

    private void OnGatherBuddy(string command, string _)
    {

    }

    private void OnGather(string command, string arguments)
    {
        if (arguments.Length == 0)
            Dalamud.Chat.Print("Please supply a (partial) item name for /gather.");
        else
            Gatherer!.OnGatherAction(arguments);
        var item = Identificator.IdentifyGatherable(arguments);
        if (item != null)
            Dalamud.Chat.Print(item.Name[Language]);
    }

    private void OnGatherBtn(string command, string arguments)
    {
        if (arguments.Length == 0)
            Dalamud.Chat.Print("Please supply a (partial) item name for /gatherbot.");
        else
            Gatherer!.OnGatherAction(arguments, GatheringType.Botanist);
    }

    private void OnGatherMin(string command, string arguments)
    {
        if (arguments.Length == 0)
            Dalamud.Chat.Print("Please supply a (partial) item name for /gathermin.");
        else
            Gatherer!.OnGatherAction(arguments, GatheringType.Miner);
    }

    private void OnGatherFish(string command, string arguments)
    {
        if (arguments.Length == 0)
            Dalamud.Chat.Print("Please supply a (partial) fish name for /gatherfish.");
        else
            Gatherer!.OnFishAction(arguments);
    }

    private void OnGatherBuddy()
        => _gatherInterface.Visible = !_gatherInterface.Visible;

    private void OnGatherGroup(string command, string arguments)
    {
        var argumentParts = arguments.Split();
        switch (argumentParts.Length)
        {
            case 0:
                Gatherer!.OnGroupGatherAction("", 0);
                break;
            case 1:
                Gatherer!.OnGroupGatherAction(argumentParts[0], 0);
                break;
            default:
                {
                    Gatherer!.OnGroupGatherAction(argumentParts[0], int.TryParse(argumentParts[1], out var offset) ? offset : 0);
                    break;
                }
        }
    }

    private void OnGatherDebug(string command, string arguments)
    {
        var argumentParts = arguments.Split();
        if (argumentParts.Length == 0)
            return;

        if (Util.CompareCi(argumentParts[0], "dump"))
            switch (argumentParts[1].ToLowerInvariant())
            {
                case "aetherytes":
                    Gatherer!.DumpAetherytes();
                    break;
                case "territories":
                    Gatherer!.DumpTerritories();
                    break;
                case "items":
                    Gatherer!.DumpItems();
                    break;
                case "nodes":
                    Gatherer!.DumpNodes();
                    break;
                case "fish":
                    Gatherer!.DumpFish();
                    break;
                case "fishingspots":
                    Gatherer!.DumpFishingSpots();
                    break;
                case "fishlog":
                    Gatherer!.FishManager.DumpFishLog();
                    break;
            }

        if (Util.CompareCi(argumentParts[0], "mergefish"))
        {
            if (argumentParts.Length < 2)
            {
                Dalamud.Chat.PrintError("Please provide a filename to merge.");
                return;
            }

            var name = arguments.Substring(argumentParts[0].Length + 1);
            var fish = Gatherer!.FishManager.MergeFishRecords(new FileInfo(name));
            switch (fish)
            {
                case -1:
                    Dalamud.Chat.PrintError($"The provided file {name} does not exist.");
                    return;
                case -2:
                    Dalamud.Chat.PrintError("Could not create a backup of your records, merge stopped.");
                    return;
                case -3:
                    Dalamud.Chat.PrintError("Unexpected error occurred, merge stopped.");
                    return;
                default:
                    Dalamud.Chat.Print($"{fish} Records updated with new data.");
                    Gatherer!.FishManager.SaveFishRecords();
                    return;
            }
        }

        if (Util.CompareCi(argumentParts[0], "purgefish"))
        {
            var name = arguments.Substring(argumentParts[0].Length + 1);
            var fish = Gatherer!.FishManager.FindFishByName(name, Language);
            if (fish == null)
                Dalamud.Chat.PrintError($"No fish found for [{name}].");
            else
                fish.Record.Delete();
        }

        if (Util.CompareCi(argumentParts[0], "weather"))
        {
            var weather = Service<SkyWatcher>.Get().GetForecast(Dalamud.ClientState.TerritoryType);
            Dalamud.Chat.Print(weather.Weather.Name);
        }

        if (Util.CompareCi(argumentParts[0], "export"))
            if (argumentParts.Length >= 2 && Util.CompareCi(argumentParts[1], "fish"))
            {
                var ids = Gatherer!.FishManager.Fish.Values.Where(Gatherer.FishManager.FishLog.IsUnlocked).Select(i => i.ItemId).ToArray();
                var output = $"Exported caught fish to clipboard ({ids.Length}/{Gatherer.FishManager.Fish.Count} caught).";
                PluginLog.Information(output);
                Dalamud.Chat.Print(output);
                ImGui.SetClipboardText(JsonConvert.SerializeObject(ids, Formatting.Indented));
            }
    }
}
