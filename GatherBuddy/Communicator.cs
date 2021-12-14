using System.Diagnostics;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using GatherBuddy.Classes;
using GatherBuddyA.Classes;
using GatherBuddyA.Structs;

namespace GatherBuddy;

public static class Communicator
{
    public delegate SeStringBuilder ReplacePlaceholder(SeStringBuilder builder, string placeholder);

    // Split a format string with '{text}' placeholders into a SeString with Payloads, 
    // and replace all placeholders by the returned payloads.
    private static SeString Format(string format, ReplacePlaceholder func)
    {
        SeStringBuilder builder     = new();
        var             lastPayload = 0;
        var             openBracket = -1;
        for (var i = 0; i < format.Length; ++i)
        {
            if (format[i] == '{')
            {
                openBracket = i;
            }
            else if (openBracket != -1 && format[i] == '}')
            {
                builder.AddText(format.Substring(lastPayload,   openBracket - lastPayload));
                var placeholder = format.Substring(openBracket, i - openBracket + 1);
                Debug.Assert(placeholder.StartsWith('{') && placeholder.EndsWith('}'));
                func(builder, placeholder);
                lastPayload = i + 1;
                openBracket = -1;
            }
        }

        if (lastPayload != format.Length)
            builder.AddText(format[lastPayload..]);
        return builder.BuiltString;
    }

    private static SeStringBuilder AddFullMapLink(SeStringBuilder builder, string name, Territory territory, float xCoord, float yCoord,
        bool openMapLink = false, float fudgeFactor = 0.05f)
    {
        var mapPayload = new MapLinkPayload(territory.Id, territory.Data.RowId, xCoord, yCoord, fudgeFactor);
        if (openMapLink)
            Dalamud.GameGui.OpenMapWithMapLink(mapPayload);

        return builder.Add(mapPayload)
            .AddUiForeground(500)
            .AddUiGlow(501)
            .AddText($"{(char)SeIconChar.LinkMarker}")
            .AddUiGlowOff()
            .AddUiForegroundOff()
            .AddText(name)
            .Add(RawPayload.LinkTerminator);
    }

    public static SeString FormatIdentifiedItemMessage(string format, string input, uint itemId)
    {
        SeStringBuilder Replace(SeStringBuilder builder, string s)
            => s.ToLowerInvariant() switch
            {
                "{id}"    => builder.AddText(itemId.ToString()),
                "{name}"  => builder.AddItemLink(itemId, false),
                "{input}" => builder.AddText(input),
                _         => builder.AddText(s),
            };

        return Format(format, Replace);
    }

    public static SeString FormatChoseFishingSpotMessage(string format, FishingSpot spot, Fish fish, Bait bait)
    {
        SeStringBuilder Replace(SeStringBuilder builder, string s)
            => s.ToLowerInvariant() switch
            {
                "{id}"       => builder.AddText(spot.Id.ToString()),
                "{name}"     => AddFullMapLink(builder, spot.Name, spot.Territory, spot.XCoord, spot.YCoord),
                "{fishid}"   => builder.AddText(fish.ItemId.ToString()),
                "{fishname}" => builder.AddItemLink(fish.ItemId, false),
                "{input}"    => builder.AddText(s),
                "{baitname}" => builder.AddItemLink(bait.Id, false),
                _            => builder.AddText(s),
            };

        return Format(format, Replace);
    }

    public static SeString FormatAlarmMessage(string format, Alarm alarm, int currentEorzeaMinute)
    {
        // TODO
        SeStringBuilder Replace(SeStringBuilder builder, string s)
            => s.ToLowerInvariant() switch
            {
                "{Name}"   => builder.AddText(alarm.Name),
                "{Offset}" => builder.AddText(alarm.MinuteOffset.ToString()),
                //"{DelayString}" => builder.AddText(DelayStringNode(alarm, currentMinute)),
                "{TimesShort}" => builder.AddText(alarm.Node!.Times!.PrintHours(true)),
                "{TimesLong}"  => builder.AddText(alarm.Node!.Times!.PrintHours()),
                //"{AllItems}" => alarm.Node!.Items!.ActualItems
                //    .SelectMany(i => ChatUtil.CreateLink(i.ItemData).Prepend(new TextPayload(", "))).Skip(1).ToList(),
                _ => builder.AddText(s),
            };

        return Format(format, Replace);
    }

}
