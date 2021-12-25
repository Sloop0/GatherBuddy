using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Logging;
using GatherBuddy.Alarms;
using GatherBuddy.Classes;
using GatherBuddy.Enums;
using GatherBuddy.Interfaces;
using GatherBuddy.SeFunctions;
using GatherBuddy.Structs;
using ImGuiNET;
using GatheringType = GatherBuddy.Enums.GatheringType;

namespace GatherBuddy;

public partial class GatherBuddy
{
    private const int ActionDelay = 200;

    private Gatherable? FindGatherableLogged(string itemName)
    {
        var item = Identificator.IdentifyGatherable(itemName);
        if (item == null)
        {
            var output = $"Could not find corresponding item to \"{itemName}\".";
            Dalamud.Chat.Print(output);
            PluginLog.Verbose(output);
            return null;
        }

        if (Config.IdentifiedItemFormat.Length > 0)
            Dalamud.Chat.Print(Communicator.FormatIdentifiedItemMessage(Config.IdentifiedItemFormat, itemName, item.ItemId));
        PluginLog.Verbose(GatherBuddyConfiguration.DefaultIdentifiedItemFormat, item.ItemId, item.Name[Language], itemName);
        return item;
    }

    private Fish? FindFishLogged(string fishName)
    {
        var fish = Identificator.IdentifyFish(fishName);
        if (fish == null)
        {
            var output = $"Could not find corresponding item to \"{fishName}\".";
            Dalamud.Chat.PrintError(output);
            PluginLog.Verbose(output);
            return null;
        }

        if (Config.IdentifiedFishFormat.Length > 0)
            Dalamud.Chat.Print(Communicator.FormatIdentifiedItemMessage(Config.IdentifiedFishFormat, fishName, fish.ItemId));
        PluginLog.Verbose(GatherBuddyConfiguration.DefaultIdentifiedFishFormat, fish.ItemId, fish.Name[Language], fishName);
        return fish;
    }

    private static ILocation? FindClosestAetheryte(IGatherable item, GatheringType? type = null)
    {
        var location =  Config.AetherytePreference switch
        {
            AetherytePreference.Distance => Identificator.FindClosestAetheryteTravel(item, type),
            AetherytePreference.Cost     => Identificator.FindClosestAetheryteCost(item, type),
            _                            => throw new ArgumentException(),
        };

        if (location != null)
            return location;

        var output = $"No associated location or attuned aetheryte found for {item.Name[Language]}.";
        Dalamud.Chat.PrintError(output);
        PluginLog.Debug(output);
        return null;

    }

    private GatheringNode? FindClosestGatheringNode(string itemName, GatheringType? type = null)
    {
        var item = FindGatherableLogged(itemName);
        if (item == null)
            return null;

        var closestLocation = FindClosestAetheryte(item, type);
        if (closestLocation == null)
            return null;

        var node = (GatheringNode)closestLocation;
        if (!Config.PrintUptime || node.Times.AlwaysUp())
            return node;

        var uptime = node.Times.NextUptime();
        if (uptime.Start > SeTime.ServerTime)
        {
            var diff    = uptime.Start.AddMilliseconds(-SeTime.ServerTime);
            var minutes = diff.CurrentMinuteOfDay;
            var seconds = diff.CurrentSecond;
            Dalamud.Chat.Print(minutes > 0
                ? $"Node is up at {node.Times.PrintHours()} (in {minutes}:{seconds:D2} Minutes)."
                : $"Node is up at {node.Times.PrintHours()} (in {seconds} Seconds).");
        }
        else
        {
            var diff    = uptime.End.AddMilliseconds(-SeTime.ServerTime);
            var minutes = diff.CurrentMinuteOfDay;
            var seconds = diff.CurrentSecond;
            Dalamud.Chat.Print(minutes > 0
                ? $"Node is up at {node.Times.PrintHours()} (for the next {minutes}:{seconds:D2} Minutes)."
                : $"Node is up at {node.Times.PrintHours()} (for the next {seconds} Seconds).");
        }

        return node;
    }

    private static async Task<bool> TeleportToLocation(ITeleportable location)
    {
        if (!Config.UseTeleport)
            return true;

        try
        {
            if (location.ClosestAetheryte == null)
            {
                PluginLog.Debug("No valid aetheryte found for {data}.", location switch
                {
                    GatheringNode n => $"gathering node {n.BaseId}",
                    FishingSpot s   => $"fishing spot {s.Id}",
                    _               => "unknown location",
                });
                return false;
            }


            Teleporter.Teleport(location.ClosestAetheryte.Id);
            await Task.Delay(ActionDelay);
        }
        catch (Exception e)
        {
            PluginLog.Error($"Error while teleporting:\n{e}");
            return false;
        }

        return true;
    }

    private static string GearForType(GatheringType type)
        => type.ToGroup() switch
        {
            GatheringType.Miner    => Config.MinerSetName,
            GatheringType.Botanist => Config.BotanistSetName,
            GatheringType.Fisher   => Config.FisherSetName,
            _                      => string.Empty,
        };

    private static async Task<bool> EquipGear(GatheringType type)
    {
        if (!Config.UseGearChange)
            return true;

        try
        {
            var set = GearForType(type);
            if (set.Length == 0)
            {
                PluginLog.Debug("No job type associated with location or no gearset configured.");
                return false;
            }

            CommandManager.Execute($"/gearset change \"{set}\"");
            await Task.Delay(ActionDelay);
        }
        catch (Exception e)
        {
            PluginLog.Error($"Error while equipping gearset:\n{e}");
            return false;
        }

        return true;
    }

    private static async Task<bool> SetLocationFlag(IMarkable location)
    {
        if (!(Config.WriteCoordinates || Config.UseCoordinates) || location.IntegralXCoord == 100 || location.IntegralYCoord == 100)
            return true;

        try
        {
            var link = Communicator.AddFullMapLink(new SeStringBuilder(), location.Name, location.Territory, location.XCoord, location.YCoord,
                true).BuiltString;
            if (Config.WriteCoordinates)
                Dalamud.Chat.Print(link);
            await Task.Delay(ActionDelay);
        }
        catch (Exception e)
        {
            PluginLog.Error($"Error while setting location flag:\n{e}");
            return false;
        }

        return true;
    }

    public async Task GatherLocation(ILocation location)
    {
        var type = location is GatheringNode n ? n.GatheringType : GatheringType.Fisher;
        if (!await EquipGear(type))
            return;
        if (!await TeleportToLocation(location))
            return;

        await SetLocationFlag(location);
    }

    public void CopyAndLinkBait(Bait bait)
    {
        if (bait.Id == 0)
            return;

        ImGui.SetClipboardText(bait.Name);
        Dalamud.Chat.Print(SeString.CreateItemLink(bait.Id, false).Append(new TextPayload(" copied to clipboard.")));
    }

    public async Task GatherItem(IGatherable? item, GatheringType? type = null)
    {
        if (item == null)
            return;

        if (Config.PrintSpearfishInfo && item is Fish { IsSpearFish: true } f)
            Dalamud.Chat.Print($"Catch {f.Size} sized fish moving {f.Speed}.");

        var closestSpot = FindClosestAetheryte(item, type);
        if (closestSpot != null)
            await GatherLocation(closestSpot);
    }

    private async Task<bool> HandleAlarmAction(string name, Alarm? alarm)
    {
        if (!Utility.Util.CompareCi(name, "alarm"))
            return false;

        if (alarm == null)
        {
            Dalamud.Chat.PrintError("No active alarm was triggered, yet.");
            return true;
        }

        SeStringBuilder builder = new();
        if (alarm.Type == AlarmType.Fish && alarm.Fish != null)
        {
            builder.AddText($"Teleporting to [Alarm {alarm.Name}] (");
            builder.AddItemLink(alarm.Fish.ItemId, false);
            builder.AddText(")");
            Dalamud.Chat.Print(builder.BuiltString);
            await GatherItem(alarm.Fish);
        }
        else if (alarm.Node != null)
        {
            builder.AddText($"Teleporting to [Alarm {alarm.Name}] ({alarm.Node.Times.PrintHours()}):\n    ");
            foreach (var item in alarm.Node.Items)
            {
                builder.AddItemLink(item.ItemId, false);
                builder.AddText(", ");
            }

            var se = builder.BuiltString;
            if (se.Payloads.Count > 1)
                se.Payloads[^1] = new TextPayload(".");
            Dalamud.Chat.Print(se);
            await GatherLocation(alarm.Node);
        }

        return true;
    }

    public async Task GatherFishByName(string fishName)
    {
        if (await HandleAlarmAction(fishName, Alarms.LastFishAlarm))
            return;

        var fish = FindFishLogged(fishName);
        await GatherItem(fish);
    }

    public async Task GatherItemByName(string itemName, GatheringType? type = null)
    {
        if (await HandleAlarmAction(itemName, Alarms.LastNodeAlarm))
            return;

        var item = FindGatherableLogged(itemName);
        await GatherItem(item, type);
    }
}
