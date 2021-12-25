using System.Linq;
using System.Text.RegularExpressions;
using Dalamud;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Logging;

namespace GatherBuddy.FishTimer.Parser;

public partial class FishingParser
{
    private void HandleCastMatch(Match match)
    {
        var tmp = match.Groups["FishingSpot"];
        var fishingSpotName = tmp.Success
            ? FishingSpotNameHacks(tmp.Value.ToLowerInvariant())
            : match.Groups["FishingSpotWithArticle"].Value.ToLowerInvariant();

        if (FishingSpotNames.TryGetValue(fishingSpotName, out var fishingSpot))
            BeganFishing?.Invoke(fishingSpot);
        // Hack against 'The' special cases.
        else if (GatherBuddy.Language == ClientLanguage.English
              && fishingSpotName.StartsWith("the ")
              && FishingSpotNames.TryGetValue(fishingSpotName[4..], out fishingSpot))
            BeganFishing?.Invoke(fishingSpot);
        else
            PluginLog.Error($"Began fishing at unknown fishing spot: \"{fishingSpotName}\".");
    }

    private void HandleSpotDiscoveredMatch(Match match)
    {
        var fishingSpotName = match.Groups["FishingSpot"].Value.ToLowerInvariant();
        if (FishingSpotNames.TryGetValue(fishingSpotName, out var fishingSpot))
            IdentifiedSpot?.Invoke(fishingSpot);
        // Hack against 'The' special cases.
        else if (GatherBuddy.Language == ClientLanguage.English
              && fishingSpotName.StartsWith("the ")
              && FishingSpotNames.TryGetValue(fishingSpotName[4..], out fishingSpot))
            IdentifiedSpot?.Invoke(fishingSpot);
        else
            PluginLog.Error($"Discovered unknown fishing spot: \"{fishingSpotName}\".");
    }

    private void HandleCatchMatch(SeString message)
    {
        var item = (ItemPayload?)message.Payloads.FirstOrDefault(p => p is ItemPayload);
        if (item == null)
        {
            PluginLog.Error("Fish caught, but no item link in message.");
            return;
        }

        if (item.ItemId == 0u)
        {
            PluginLog.Error("Caught unknown fish with unknown id.");
            return;
        }

        // Check against collectibles.
        var id = item.ItemId > 500000 ? item.ItemId - 500000 : item.ItemId;

        if (GatherBuddy.GameData.Fishes.TryGetValue(id, out var fish))
            CaughtFish?.Invoke(fish);
        else
            PluginLog.Error($"Caught unknown fish with id {id}.");
    }

    private const XivChatType FishingMessage      = (XivChatType)2243;
    private const XivChatType FishingCatchMessage = (XivChatType)2115;


    private void OnMessageDelegate(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        switch (type)
        {
            case FishingMessage:
            {
                var text = message.TextValue;

                if (text == _regexes.Bite)
                {
                    SomethingBit?.Invoke();
                    return;
                }

                if (text.Contains(_regexes.Undiscovered))
                {
                    BeganFishing?.Invoke(null);
                    return;
                }

                var match = _regexes.Cast.Match(text);
                if (match.Success)
                {
                    HandleCastMatch(match);
                    return;
                }

                match = _regexes.Mooch.Match(text);
                if (match.Success)
                {
                    BeganMooching?.Invoke();
                    return;
                }

                match = _regexes.AreaDiscovered.Match(text);
                if (match.Success)
                    HandleSpotDiscoveredMatch(match);
                break;
            }
            case FishingCatchMessage:
            {
                var text = message.TextValue;
                if (_regexes.Catch.Match(text).Success || _regexes.NoCatchFull.Match(text).Success)
                    HandleCatchMatch(message);
                break;
            }
        }
    }
}
