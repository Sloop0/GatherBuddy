﻿using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Configuration;
using Dalamud.Game.Text;
using GatherBuddy.Enums;

namespace GatherBuddy.Config;

public partial class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 4;

    // set names
    public string BotanistSetName { get; set; } = "BTN";
    public string MinerSetName    { get; set; } = "MIN";
    public string FisherSetName   { get; set; } = "FSH";

    // formats
    public string IdentifiedItemFormat { get; set; } = DefaultIdentifiedItemFormat;
    public string IdentifiedFishFormat { get; set; } = DefaultIdentifiedFishFormat;
    public string NodeAlarmFormat      { get; set; } = DefaultNodeAlarmFormat;
    public string FishAlarmFormat      { get; set; } = DefaultFishAlarmFormat;


    // Interface
    public AetherytePreference AetherytePreference { get; set; } = AetherytePreference.Distance;
    public TabSortOrder        TabSortOrder        { get; set; } = TabSortOrder.ItemFishWeather;
    public ItemFilter          ShowItems           { get; set; } = ItemFilter.All;
    public FishFilter          ShowFish            { get; set; } = FishFilter.All;
    public PatchFlag           HideFishPatch       { get; set; } = 0;
    public JobFlags            LocationFilter      { get; set; } = (JobFlags)0x3F;


    // General Config
    public bool        OpenOnStart           { get; set; } = false;
    public bool        UseGearChange         { get; set; } = true;
    public bool        UseTeleport           { get; set; } = true;
    public bool        UseCoordinates        { get; set; } = true;
    public bool        WriteCoordinates      { get; set; } = true;
    public bool        PrintUptime           { get; set; } = true;
    public bool        PrintSpearfishInfo    { get; set; } = true;
    public bool        SkipTeleportIfClose   { get; set; } = true;
    public XivChatType ChatTypeMessage       { get; set; } = XivChatType.Echo;
    public XivChatType ChatTypeError         { get; set; } = XivChatType.ErrorMessage;
    public bool        AddIngameContextMenus { get; set; } = true;

    // Weather tab
    public bool ShowWeatherNames { get; set; } = true;

    // Alarms
    public bool AlarmsEnabled { get; set; } = false;
    public bool AlarmsInDuty  { get; set; } = true;

    // Colors
    public Dictionary<ColorId, uint> Colors { get; set; }
        = Enum.GetValues<ColorId>().ToDictionary(c => c, c => c.Data().DefaultColor);

    public int SeColorHighlight1 = DefaultSeColorHighlight1;
    public int SeColorHighlight2 = DefaultSeColorHighlight2;
    public int SeColorHighlight3 = DefaultSeColorHighlight3;

    // Fish Timer
    public bool ShowFishTimer        { get; set; } = true;
    public bool FishTimerEdit        { get; set; } = true;
    public bool HideUncaughtFish     { get; set; } = false;
    public bool HideUnavailableFish  { get; set; } = false;
    public bool ShowFishTimerUptimes { get; set; } = true;

    // Spearfish Helper
    public bool ShowSpearfishHelper          { get; set; } = true;
    public bool ShowSpearfishNames           { get; set; } = true;
    public bool ShowAvailableSpearfish       { get; set; } = true;
    public bool ShowSpearfishSpeed           { get; set; } = false;
    public bool ShowSpearfishCenterLine      { get; set; } = true;
    public bool ShowSpearfishListIconsAsText { get; set; } = false;


    // Gather Window
    public bool ShowGatherWindow                { get; set; } = true;
    public bool ShowGatherWindowTimers          { get; set; } = true;
    public bool ShowGatherWindowAlarms          { get; set; } = true;
    public bool SortGatherWindowByUptime        { get; set; } = false;
    public bool HideGatherWindowInDuty          { get; set; } = true;
    public bool OnlyShowGatherWindowHoldingCtrl { get; set; } = false;
    public bool LockGatherWindow                { get; set; } = false;

    public void Save()
        => Dalamud.PluginInterface.SavePluginConfig(this);


    // Add missing colors to the dictionary if necessary.
    private void AddColors()
    {
        var save = false;
        foreach (var color in Enum.GetValues<ColorId>())
            save |= Colors.TryAdd(color, color.Data().DefaultColor);
        if (save)
            Save();
    }

    public static Configuration Load()
    {
        if (Dalamud.PluginInterface.GetPluginConfig() is Configuration config)
        {
            config.AddColors();
            return config;
        }

        config = new Configuration();
        config.Save();
        return config;
    }
}
