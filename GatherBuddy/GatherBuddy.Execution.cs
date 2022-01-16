using System;
using System.Linq;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Logging;
using GatherBuddy.Classes;
using GatherBuddy.Config;
using GatherBuddy.Enums;
using GatherBuddy.Interfaces;
using GatherBuddy.Managers;
using GatherBuddy.SeFunctions;
using GatherBuddy.Time;
using GatherBuddy.Utility;
using CommandManager = GatherBuddy.Managers.CommandManager;
using GatheringType = GatherBuddy.Enums.GatheringType;

namespace GatherBuddy;

public class Executor
{
    private enum IdentifyType
    {
        None,
        Item,
        Fish,
    }

    private readonly CommandManager _commandManager = new(Dalamud.GameGui, Dalamud.SigScanner);
    private readonly MacroManager   _macroManager   = new();
    public readonly  Identificator  Identificator   = new();

    private IdentifyType _identifyType = IdentifyType.None;
    private string       _name         = string.Empty;

    private IGatherable? _item = null;

    private GatheringType? _gatheringType = null;
    private ILocation?     _location      = null;
    private TimeInterval   _uptime        = TimeInterval.Always;

    private static void ItemNotFound(string itemName)
    {
        var output = $"Could not find corresponding item to \"{itemName}\".";
        Dalamud.Chat.Print(output);
        PluginLog.Verbose(output);
    }

    private void LocationNotFound()
    {
        var output =
            $"No associated location or attuned aetheryte found for {_item?.Name[GatherBuddy.Language] ?? "Unknown"}{(_gatheringType == null ? "." : $" with condition {_gatheringType.Value}.")}";
        Dalamud.Chat.PrintError(output);
        PluginLog.Debug(output);
    }

    private void FindGatherableLogged(string itemName)
    {
        _item = Identificator.IdentifyGatherable(itemName);
        if (_item == null)
        {
            ItemNotFound(itemName);
            return;
        }

        if (GatherBuddy.Config.IdentifiedItemFormat.Length > 0)
            Dalamud.Chat.Print(Communicator.FormatIdentifiedItemMessage(GatherBuddy.Config.IdentifiedItemFormat, itemName, _item));
        PluginLog.Verbose(Configuration.DefaultIdentifiedItemFormat, _item.ItemId, _item.Name[GatherBuddy.Language], itemName);
    }

    private void FindFishLogged(string fishName)
    {
        _item = Identificator.IdentifyFish(fishName);
        if (_item == null)
        {
            ItemNotFound(fishName);
            return;
        }

        if (GatherBuddy.Config.IdentifiedFishFormat.Length > 0)
            Dalamud.Chat.Print(Communicator.FormatIdentifiedItemMessage(GatherBuddy.Config.IdentifiedFishFormat, fishName, _item));
        PluginLog.Verbose(Configuration.DefaultIdentifiedFishFormat, _item.ItemId, _item.Name[GatherBuddy.Language], fishName);
    }

    private void DoIdentify()
    {
        if (_name.Length == 0)
            return;

        switch (_identifyType)
        {
            case IdentifyType.None: return;
            case IdentifyType.Item:
                FindGatherableLogged(_name);
                return;
            case IdentifyType.Fish:
                FindFishLogged(_name);
                return;
            default: throw new ArgumentOutOfRangeException();
        }
    }

    private void FindClosestLocation()
    {
        if (_item == null)
            return;

        _location = null;
        if (_gatheringType == null || _item is Fish)
            (_location, _uptime) = GatherBuddy.UptimeManager.BestLocation(_item);
        else
            (_location, _uptime) = GatherBuddy.UptimeManager.NextUptime((Gatherable)_item, _gatheringType.Value, GatherBuddy.Time.ServerTime);

        if (_location == null)
            LocationNotFound();
    }

    private void DoTeleport()
    {
        if (!GatherBuddy.Config.UseTeleport || _location?.ClosestAetheryte == null)
            return;

        if (GatherBuddy.Config.SkipTeleportIfClose
         && Dalamud.ClientState.TerritoryType == _location.Territory.Id
         && Dalamud.ClientState.LocalPlayer != null)
        {
            // TODO verify
            var posX = Maps.NodeToMap(Dalamud.ClientState.LocalPlayer.Position.X, _location.Territory.SizeFactor);
            var posY = Maps.NodeToMap(Dalamud.ClientState.LocalPlayer.Position.Y, _location.Territory.SizeFactor);
            if (_location.ClosestAetheryte.WorldDistance(_location.Territory.Id, posX, posY)
              < _location.ClosestAetheryte.WorldDistance(_location.Territory.Id, _location.IntegralXCoord, _location.IntegralYCoord) * 1.5)
                return;
        }

        TeleportToAetheryte(_location.ClosestAetheryte);
    }

    private void DoGearChange()
    {
        if (!GatherBuddy.Config.UseGearChange || _location == null)
            return;

        var set = _location.GatheringType.ToGroup() switch
        {
            GatheringType.Fisher   => GatherBuddy.Config.FisherSetName,
            GatheringType.Botanist => GatherBuddy.Config.BotanistSetName,
            GatheringType.Miner    => GatherBuddy.Config.MinerSetName,
            _                      => string.Empty,
        };
        if (set.Length == 0)
        {
            PluginLog.Debug("No job type associated with location or no gearset configured.");
            Dalamud.Chat.PrintError("No job type associated with location or no gearset configured.");
            return;
        }

        _commandManager.Execute($"/gearset change \"{set}\"");
    }


    private void DoMapFlag()
    {
        if (!GatherBuddy.Config.WriteCoordinates && !GatherBuddy.Config.UseCoordinates || _location == null)
            return;

        if (_location.IntegralXCoord == 100 || _location.IntegralYCoord == 100)
            return;

        var link = Communicator
            .AddFullMapLink(new SeStringBuilder(), _location.Name, _location.Territory, _location.XCoord, _location.YCoord, true).BuiltString;
        if (GatherBuddy.Config.WriteCoordinates)
            Dalamud.Chat.Print(link);
    }

    private void DoAdditionalInfo()
    {
        if (GatherBuddy.Config.PrintSpearfishInfo && _item is Fish { IsSpearFish: true } f)
            Dalamud.Chat.Print($"Catch {f.Size} sized fish moving {f.Speed}.");

        if (GatherBuddy.Config.PrintUptime && !_uptime.Equals(TimeInterval.Always))
        {
            if (_uptime.Start > GatherBuddy.Time.ServerTime)
            {
                var diff    = _uptime.Start.AddMilliseconds(-GatherBuddy.Time.ServerTime);
                var minutes = diff.CurrentMinuteOfDay;
                var seconds = diff.CurrentSecond;
                Dalamud.Chat.Print(minutes > 0
                    ? $"Next up in {minutes}:{seconds:D2} Minutes."
                    : $"Next up in {seconds} Seconds.");
            }
            else
            {
                var diff    = _uptime.End.AddMilliseconds(-GatherBuddy.Time.ServerTime);
                var minutes = diff.CurrentMinuteOfDay;
                var seconds = diff.CurrentSecond;
                Dalamud.Chat.Print(minutes > 0
                    ? $"Currently up for the next {minutes}:{seconds:D2} Minutes."
                    : $"Currently up for the next {seconds} Seconds.");
            }
        }
    }

    public bool DoCommand(string argument)
    {
        switch (argument)
        {
            case GatherBuddy.IdentifyCommand:
                DoIdentify();
                FindClosestLocation();
                return true;
            case GatherBuddy.MapMarkerCommand:
                DoMapFlag();
                return true;
            case GatherBuddy.GearChangeCommand:
                DoGearChange();
                return true;
            case GatherBuddy.TeleportCommand:
                DoTeleport();
                return true;
            case GatherBuddy.AdditionalInfoCommand:
                DoAdditionalInfo();
                return true;
            default: return false;
        }
    }

    public void GatherLocation(ILocation location)
    {
        _identifyType  = IdentifyType.None;
        _name          = string.Empty;
        _item          = null;
        _gatheringType = location.GatheringType.ToGroup();
        _location      = location;
        if (location is GatheringNode n)
            _uptime = n.Times.NextUptime(GatherBuddy.Time.ServerTime);
        else
            _uptime = TimeInterval.Always;

        _macroManager.Execute();
    }

    public void GatherItem(IGatherable? item, GatheringType? type = null)
    {
        if (item == null)
            return;

        _identifyType  = IdentifyType.None;
        _name          = string.Empty;
        _item          = item;
        _location      = null;
        _gatheringType = type?.ToGroup();
        _uptime        = TimeInterval.Always;

        _macroManager.Execute();
    }

    public void GatherFishByName(string fishName)
    {
        if (fishName.Length == 0)
            return;

        _identifyType  = IdentifyType.Fish;
        _name          = fishName;
        _item          = null;
        _location      = null;
        _gatheringType = null;
        _uptime        = TimeInterval.Always;

        _macroManager.Execute();
    }

    public void GatherItemByName(string itemName, GatheringType? type = null)
    {
        if (itemName.Length == 0)
            return;

        _identifyType  = IdentifyType.Item;
        _name          = itemName;
        _item          = null;
        _location      = null;
        _gatheringType = type;
        _uptime        = TimeInterval.Always;

        _macroManager.Execute();
    }

    public static void TeleportToAetheryte(Aetheryte aetheryte)
    {
        if (aetheryte.Id == 0)
            return;

        if (Teleporter.IsAttuned(aetheryte.Id))
            Teleporter.TeleportUnchecked(aetheryte.Id);
        else
            Dalamud.Chat.PrintError($"Not attuned to chosen aetheryte {aetheryte.Name}.");
    }

    public static void TeleportToTerritory(Territory territory)
    {
        if (territory.Aetherytes.Count == 0)
        {
            Dalamud.Chat.PrintError($"{territory.Name} has no valid aetheryte.");
            return;
        }

        var aetheryte = territory.Aetherytes.FirstOrDefault(a => Teleporter.IsAttuned(a.Id));
        if (aetheryte == null)
        {
            Dalamud.Chat.PrintError($"Not attuned to any aetheryte in {territory.Name}.");
            return;
        }

        Teleporter.TeleportUnchecked(aetheryte.Id);
    }
}
