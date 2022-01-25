﻿using System;

namespace GatherBuddy.Config;

public enum ColorId
{
    AvailableItem,
    UpcomingItem,
    DependentAvailableFish,
    DependentUpcomingFish,
    DisabledText,
    WarningBg,
    ChangedLocationBg,

    FishTimerWeakTug,
    FishTimerStrongTug,
    FishTimerLegendaryTugPrecision,
    FishTimerLegendaryTugPowerful,
    FishTimerUnknown,
    FishTimerUnavailable,
    FishTimerBackground,
    FishTimerProgress,
    FishTimerMarkers,
    FishTimerText,

    HeaderEorzeaTime,
    HeaderNextHour,
    HeaderWeather,

    WeatherTabCurrent,
    WeatherTabLast,
    WeatherTabHeaderCurrent,
    WeatherTabHeaderLast,

    SpearfishHelperBackgroundFish,
    SpearfishHelperBackgroundList,
    SpearfishHelperCenterLine,
    SpearfishHelperTextFish,
}

public static class ColorIdExtensions
{
    // @formatter:off
    public static (uint DefaultColor, string Name, string Description) Data(this ColorId color)
        => color switch
        {
            ColorId.AvailableItem                  => (0xFF20B020, "Interface Available Items",                           "Items that are always or currently available."),
            ColorId.UpcomingItem                   => (0xFF20B0A0, "Interface Upcoming Items",                            "Items that are not currently available."),
            ColorId.DependentAvailableFish         => (0xFFFF3030, "Interface Dependent Available Fish",                  "Fish that are currently available based on their own conditions, but require intuition or mooches that have other requirements."),
            ColorId.DependentUpcomingFish          => (0xFFA020A0, "Interface Dependent Upcoming Fish",                   "Fish that are not currently available based on their own conditions, but also require intuition or mooches that have other requirements."),
            ColorId.DisabledText                   => (0xFF606060, "Interface Disabled Text",                             "Text for disabled objects in selectors."),
            ColorId.WarningBg                      => (0xA00000A0, "Warning Background",                                  "The background color of warning badges in the user interface."),
            ColorId.ChangedLocationBg              => (0x80009000, "Custom Location Data Background",                     "The background of customly set aetherytes or coordinates for specific locations."),

            ColorId.FishTimerWeakTug               => (0x8000A000, "Fish Timer Window Weak Tug",                          "Fish that bite with a weak tug (!)."),
            ColorId.FishTimerStrongTug             => (0x8000A0A0, "Fish Timer Window Strong Tug",                        "Fish that bite with a strong tug (!!)."),
            ColorId.FishTimerLegendaryTugPrecision => (0x8000A080, "Fish Timer Window Legendary Tug (Precision Hookset)", "Fish that bite with a legendary tug (!!!) and require Precision Hookset if in Patience."),
            ColorId.FishTimerLegendaryTugPowerful  => (0x800080A0, "Fish Timer Window Legendary Tug (Powerful Hookset)",  "Fish that bite with a legendary tug (!!!) and require Powerful Hookset if in Patience."),
            ColorId.FishTimerUnknown               => (0x80404040, "Fish Timer Window Unknown Tug",                       "Fish that bite with an unknown tug."),
            ColorId.FishTimerUnavailable           => (0x800000A0, "Fish Timer Window Unavailable Fish",                  "Fish that are currently unavailable due to their conditions."),
            ColorId.FishTimerProgress              => (0xFF000000, "Fish Timer Window Progress Bar",                      "The straight line that indicates current time progress."),
            ColorId.FishTimerMarkers               => (0xFF0000FF, "Fish Timer Window Bitetime Highlight Markers",        "The two lines that indicate the beginning and end of the recorded bite window for a specific fish."),
            ColorId.FishTimerText                  => (0xFFFFFFFF, "Fish Timer Window Text",                              "Text in the fish timer window."),
            ColorId.FishTimerBackground            => (0x80000000, "Fish Timer Window Background",                        "The background of the fish timer window."),
            
            ColorId.HeaderEorzeaTime               => (0xFF008080, "Header Eorzea Time Background",                       "The background of the Eorzea Time field in the main interface header."),
            ColorId.HeaderNextHour                 => (0xFF404040, "Header Next Eorzea Hour Background",                  "The background of the Time to Next Eorzea Hour field in the main interface header."),
            ColorId.HeaderWeather                  => (0xFFA0A000, "Header Next Eorzea Background",                       "The background of the Time to Next Weather field in the main interface header."),
                                                                                                                          
            ColorId.WeatherTabCurrent             => (0x1000FF00, "Weather Tab Current Weather Column",                   "The background color of the cells depicting the current weather in the weather tab."),
            ColorId.WeatherTabLast                => (0x100000FF, "Weather Tab Last Weather Column",                      "The background color of the cells depicting the last weather in the weather tab."),
            ColorId.WeatherTabHeaderCurrent       => (0xFF008000, "Weather Tab Current Weather Header",                   "The background color of header cell depicting the starting time of the current weather in the weather tab."),
            ColorId.WeatherTabHeaderLast          => (0xFF000080, "Weather Tab Last Weather Header",                      "The background color of header cell depicting the starting time of the last weather in the weather tab."),

            ColorId.SpearfishHelperBackgroundFish => (0x40000000, "Spearfish Helper Fish Background",                     "The background color of each fish name overlayed onto the moving fish in the spearfish helper."),
            ColorId.SpearfishHelperBackgroundList => (0x80000000, "Spearfish Helper List Background",                     "The background color of the list of available fish in the spearfish helper."),
            ColorId.SpearfishHelperCenterLine     => (0xFF0000C0, "Spearfish Helper Center Line",                         "The color of the line straight up from the center of the spear."),
            ColorId.SpearfishHelperTextFish       => (0xFFFFFFFF, "Spearfish Helper Fish Text",                           "The text color of each fish name overlayed onto the moving fish in the spearfish helper."),
            _                                     => throw new ArgumentOutOfRangeException(nameof(color), color, null),
        };
    // @formatter:on

    public static uint Value(this ColorId color)
        => GatherBuddy.Config.Colors.TryGetValue(color, out var value) ? value : color.Data().DefaultColor;
}
