using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.Command;
using Dalamud.Logging;
using GatherBuddy.Enums;
using GatherBuddy.Time;

namespace GatherBuddy;

public partial class GatherBuddy
{
    public const string IdentifyCommand       = "identify";
    public const string GearChangeCommand     = "gearchange";
    public const string TeleportCommand       = "teleport";
    public const string MapMarkerCommand      = "mapmarker";
    public const string AdditionalInfoCommand = "information";
    public const string FullIdentify          = $"/gatherbuddy {IdentifyCommand}";
    public const string FullGearChange        = $"/gatherbuddy {GearChangeCommand}";
    public const string FullTeleport          = $"/gatherbuddy {TeleportCommand}";
    public const string FullMapMarker         = $"/gatherbuddy {MapMarkerCommand}";
    public const string FullAdditionalInfo    = $"/gatherbuddy {AdditionalInfoCommand}";

    private readonly Dictionary<string, CommandInfo> _commands = new();

    private void InitializeCommands()
    {
        _commands["/gatherbuddy"] = new CommandInfo(OnGatherBuddy)
        {
            HelpMessage = "Use to open the GatherBuddy interface.",
            ShowInHelp  = true,
        };

        _commands["/gather"] = new CommandInfo(OnGather)
        {
            HelpMessage = "Mark the nearest node containing the item supplied, teleport to the nearest aetheryte, equip appropriate gear.",
            ShowInHelp  = true,
        };

        _commands["/gatherbtn"] = new CommandInfo(OnGatherBtn)
        {
            HelpMessage =
                "Mark the nearest botanist node containing the item supplied, teleport to the nearest aetheryte, equip appropriate gear.",
            ShowInHelp = true,
        };

        _commands["/gathermin"] = new CommandInfo(OnGatherMin)
        {
            HelpMessage =
                "Mark the nearest miner node containing the item supplied, teleport to the nearest aetheryte, equip appropriate gear.",
            ShowInHelp = true,
        };

        _commands["/gatherfish"] = new CommandInfo(OnGatherFish)
        {
            HelpMessage =
                "Mark the nearest fishing spot containing the fish supplied, teleport to the nearest aetheryte and equip fishing gear.",
            ShowInHelp = true,
        };

        _commands["/gathergroup"] = new CommandInfo(OnGatherGroup)
        {
            HelpMessage = "Teleport to the node of a group corresponding to current time. Use /gathergroup for more details.",
            ShowInHelp  = true,
        };

        _commands["/gatherdebug"] = new CommandInfo(OnGatherDebug)
        {
            HelpMessage = "Dump some collected information.",
            ShowInHelp  = false,
        };

        foreach (var (command, info) in _commands)
            Dalamud.Commands.AddHandler(command, info);
    }

    private void DisposeCommands()
    {
        foreach (var command in _commands.Keys)
            Dalamud.Commands.RemoveHandler(command);
    }

    private void OnGatherBuddy(string command, string arguments)
    {
        if (!Executor.DoCommand(arguments))
            Interface.Toggle();
    }

    private void OnGather(string command, string arguments)
    {
        if (arguments.Length == 0)
            Dalamud.Chat.Print($"Please supply a (partial) item name for {command}.");
        else
            Executor.GatherItemByName(arguments);
    }

    private void OnGatherBtn(string command, string arguments)
    {
        if (arguments.Length == 0)
            Dalamud.Chat.Print($"Please supply a (partial) item name for {command}.");
        else
            Executor.GatherItemByName(arguments, GatheringType.Botanist);
    }

    private void OnGatherMin(string command, string arguments)
    {
        if (arguments.Length == 0)
            Dalamud.Chat.Print($"Please supply a (partial) item name for {command}.");
        else
            Executor.GatherItemByName(arguments, GatheringType.Miner);
    }

    private void OnGatherFish(string command, string arguments)
    {
        if (arguments.Length == 0)
            Dalamud.Chat.Print($"Please supply a (partial) fish name for {command}.");
        else
            Executor.GatherFishByName(arguments);
    }

    private void OnGatherGroup(string command, string arguments)
    {
        var argumentParts = arguments.Split();
        switch (argumentParts.Length)
        {
            case 0:
                Dalamud.Chat.Print($"Nope");
                break;
            case 1:
            {
                if (!GatherGroupManager.Groups.TryGetValue(argumentParts[0], out var group))
                {
                    Dalamud.Chat.Print($"Nope");
                }
                else
                {
                    var node = group.CurrentNode((uint)Time.EorzeaMinuteOfDay);
                    if (node == null)
                        Dalamud.Chat.Print($"Nope");
                    else
                        Executor.GatherItem(node.Item);
                }

                break;
            }
            default:
            {
                if (!GatherGroupManager.Groups.TryGetValue(argumentParts[0], out var group))
                {
                    Dalamud.Chat.Print($"Nope");
                }
                else
                {
                    var node = group.CurrentNode(
                        (uint)(Time.EorzeaMinuteOfDay + (int.TryParse(argumentParts[1], out var offset) ? offset : 0))
                      % RealTime.MinutesPerDay);
                    if (node == null)
                        Dalamud.Chat.Print($"Nope");
                    else
                        Executor.GatherItem(node.Item);
                }

                break;
            }
        }
    }

    private void OnGatherDebug(string command, string arguments)
    {
        var argumentParts = arguments.Split();
        if (argumentParts.Length == 0)
            return;

        var sums = Enum.GetValues<SpearfishSpeed>().ToDictionary(s => s, _ => 0);
        foreach (var fish in GatherBuddy.GameData.Fishes.Values)
            sums[fish.Speed]++;

        foreach(var (speed, sum) in sums.Where(s => s.Key != SpearfishSpeed.None && s.Key != SpearfishSpeed.Unknown))
            PluginLog.Information($"{speed.ToName(),-20} ({(ushort) speed,3}) - {sum,4}");
        //if (Util.CompareCi(argumentParts[0], "dump"))
        //    switch (argumentParts[1].ToLowerInvariant())
        //    {
        //        case "aetherytes":
        //            Gatherer!.DumpAetherytes();
        //            break;
        //        case "territories":
        //            Gatherer!.DumpTerritories();
        //            break;
        //        case "items":
        //            Gatherer!.DumpItems();
        //            break;
        //        case "nodes":
        //            Gatherer!.DumpNodes();
        //            break;
        //        case "fish":
        //            Gatherer!.DumpFish();
        //            break;
        //        case "fishingspots":
        //            Gatherer!.DumpFishingSpots();
        //            break;
        //        case "fishlog":
        //            Gatherer!.FishManager.DumpFishLog();
        //            break;
        //    }
        //
        //if (Util.CompareCi(argumentParts[0], "mergefish"))
        //{
        //    if (argumentParts.Length < 2)
        //    {
        //        Dalamud.Chat.PrintError("Please provide a filename to merge.");
        //        return;
        //    }
        //
        //    var name = arguments.Substring(argumentParts[0].Length + 1);
        //    var fish = Gatherer!.FishManager.MergeFishRecords(new FileInfo(name));
        //    switch (fish)
        //    {
        //        case -1:
        //            Dalamud.Chat.PrintError($"The provided file {name} does not exist.");
        //            return;
        //        case -2:
        //            Dalamud.Chat.PrintError("Could not create a backup of your records, merge stopped.");
        //            return;
        //        case -3:
        //            Dalamud.Chat.PrintError("Unexpected error occurred, merge stopped.");
        //            return;
        //        default:
        //            Dalamud.Chat.Print($"{fish} Records updated with new data.");
        //            Gatherer!.FishManager.SaveFishRecords();
        //            return;
        //    }
        //}
        //
        //if (Util.CompareCi(argumentParts[0], "purgefish"))
        //{
        //    var name = arguments.Substring(argumentParts[0].Length + 1);
        //    var fish = Gatherer!.FishManager.FindFishByName(name, Language);
        //    if (fish == null)
        //        Dalamud.Chat.PrintError($"No fish found for [{name}].");
        //    else
        //        fish.Record.Delete();
        //}
        //
        //if (Util.CompareCi(argumentParts[0], "weather"))
        //{
        //    var weather = Service<SkyWatcher>.Get().GetForecast(Dalamud.ClientState.TerritoryType);
        //    Dalamud.Chat.Print(weather.Weather.Name);
        //}
        //
        //if (Util.CompareCi(argumentParts[0], "export"))
        //    if (argumentParts.Length >= 2 && Util.CompareCi(argumentParts[1], "fish"))
        //    {
        //        var ids = Gatherer!.FishManager.Fish.Values.Where(Gatherer.FishManager.FishLog.IsUnlocked).Select(i => i.ItemId).ToArray();
        //        var output = $"Exported caught fish to clipboard ({ids.Length}/{Gatherer.FishManager.Fish.Count} caught).";
        //        PluginLog.Information(output);
        //        Dalamud.Chat.Print(output);
        //        ImGui.SetClipboardText(JsonConvert.SerializeObject(ids, Formatting.Indented));
        //    }
    }
}
