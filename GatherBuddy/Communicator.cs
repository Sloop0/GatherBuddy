using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using GatherBuddy.Alarms;
using GatherBuddy.Classes;
using GatherBuddy.SeFunctions;
using GatherBuddy.Structs;
using GatherBuddy.Time;

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

    public static SeStringBuilder AddFullMapLink(SeStringBuilder builder, string name, Territory territory, float xCoord, float yCoord,
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

    private static SeStringBuilder AddItemLinks(SeStringBuilder builder, IList<Gatherable> items)
    {
        for (var i = 0; i < items.Count - 1; ++i)
        {
            builder.AddItemLink(items[i].ItemId, false);
            builder.AddText(", ");
        }

        if (items.Count > 0)
            builder.AddItemLink(items.Last().ItemId, false);
        return builder;
    }

    public static SeString FormatAlarmMessage(string format, Alarm alarm, uint currentEorzeaMinute)
    {
        SeStringBuilder NodeReplace(SeStringBuilder builder, string s)
            => s.ToLowerInvariant() switch
            {
                "{name}"        => builder.AddText(alarm.Name),
                "{offset}"      => builder.AddText(alarm.MinuteOffset.ToString()),
                "{delaystring}" => builder.AddText(DelayStringNode(alarm, currentEorzeaMinute)),
                "{timesshort}"  => builder.AddText(alarm.Node!.Times.PrintHours(true)),
                "{timeslong}"   => builder.AddText(alarm.Node!.Times.PrintHours()),
                "{location}" => AddFullMapLink(builder, alarm.Node!.Name, alarm.Node.Territory, (float)alarm.Node.XCoord,
                    (float)alarm.Node.YCoord),
                "{allitems}" => AddItemLinks(builder, alarm.Node!.Items),
                _            => builder.AddText(s),
            };

        SeStringBuilder FishReplace(SeStringBuilder builder, string s)
            => s.ToLowerInvariant() switch
            {
                "{name}"        => builder.AddText(alarm.Name),
                "{offset}"      => builder.AddText(alarm.MinuteOffset.ToString()),
                "{delaystring}" => builder.AddText(DelayStringFish(alarm)),
                "{fishingspotname}" => AddFullMapLink(builder, alarm.Fish!.FishingSpots.First().Name, alarm.Fish.FishingSpots.First().Territory,
                    alarm.Fish.FishingSpots.First().XCoord,
                    alarm.Fish.FishingSpots.First().YCoord),
                "{baitname}" => alarm.Fish!.InitialBait.Id == 0
                    ? builder.AddText("Unknown Bait")
                    : builder.AddItemLink(alarm.Fish!.InitialBait.Id, false),
                "{fishname}" => builder.AddItemLink(alarm.Fish!.ItemId, false),
                _            => builder.AddText(s),
            };

        return alarm.Type == AlarmType.Node ? Format(format, NodeReplace) : Format(format, FishReplace);
    }

    private static string DelayStringFish(Alarm alarm)
    {
        var uptime    = alarm.Fish!.NextUptime();
        var ret       = "is currently up";
        var timeStamp = SeTime.ServerTime;
        if (uptime.Start <= timeStamp)
            return ret;

        var diff = uptime.Start - timeStamp;
        ret = $"will be up in {diff / RealTime.SecondsPerMinute}:{diff % RealTime.SecondsPerMinute:D2} minutes";

        return ret;
    }

    private static string DelayStringNode(Alarm alarm, uint currentMinute)
    {
        var ret = "is currently up";
        if (alarm.MinuteOffset <= 0)
            return ret;

        var hour       = currentMinute / RealTime.MinutesPerHour;
        var hourOfDay  = (int)hour % RealTime.HoursPerDay;
        var nextUptime = alarm.Node!.Times!.NextUptime(hourOfDay);
        var offTime    = (hour + nextUptime) * RealTime.MinutesPerHour - currentMinute;
        var (m, s) = EorzeaTimeStampExtensions.MinutesToReal(offTime);
        if (offTime > 0)
            ret = $"will be up in {m}:{s:D2} minutes";

        return ret;
    }
}
