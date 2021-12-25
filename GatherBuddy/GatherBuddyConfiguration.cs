﻿using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Configuration;
using GatherBuddy.Alarms;
using GatherBuddy.Gui;

namespace GatherBuddy
{
    [Flags]
    public enum ShowNodes
    {
        NoNodes  = 0,
        Mining   = 0x0001,
        Botanist = 0x0002,

        Ephemeral = 0x0004,
        Unspoiled = 0x0008,

        ARealmReborn   = 0x0010,
        Heavensward    = 0x0020,
        Stormblood     = 0x0040,
        Shadowbringers = 0x0080,
        Endwalker      = 0x0100,

        AllNodes = 0x01FF,
    }

    public enum FishSortOrder
    {
        EndTime,
        Name,
        BaitName,
        FishingSpotName,
        ZoneName,
        Uptime,
        InverseEndTime,
        InverseName,
        InverseBaitName,
        InverseFishingSpotName,
        InverseZoneName,
        InverseUptime,
    }


    public enum AetherytePreference : byte
    {
        Cost,
        Distance,
    }

    [Serializable]
    public class GatherBuddyConfiguration : IPluginConfiguration
    {
        public void MigrateV2ToV3()
        {
            if (Version > 2)
                return;

            const string oldDefaultNodeAlarmFormat      = "[GatherBuddy][Alarm {Name}]: The gathering node for {AllItems} {DelayString}.";

            if (NodeAlarmFormat == oldDefaultNodeAlarmFormat)
                NodeAlarmFormat = DefaultNodeAlarmFormat;

            WriteCoordinates = UseCoordinates;

            Version = 3;
        }


        public const string DefaultIdentifiedItemFormat        = "Identified [{Id}: {Name}] for \"{Input}\".";
        public const string DefaultIdentifiedFishFormat        = "Identified [{Id}: {Name}] for \"{Input}\".";

        public const string DefaultNodeAlarmFormat =
            "[GatherBuddy][Alarm {Name}]: The gathering node for {AllItems} {DelayString} at {Location}.";

        public const string DefaultFishAlarmFormat =
            "[GatherBuddy][Alarm {Name}]: The fish {FishName} at {FishingSpotName} {DelayString}. Catch with {BaitName}.";


        public string BotanistSetName { get; set; } = "BTN";
        public string MinerSetName    { get; set; } = "MIN";
        public string FisherSetName   { get; set; } = "FSH";

        public string IdentifiedItemFormat        { get; set; } = DefaultIdentifiedItemFormat;
        public string IdentifiedFishFormat        { get; set; } = DefaultIdentifiedFishFormat;
        public string NodeAlarmFormat             { get; set; } = DefaultNodeAlarmFormat;
        public string FishAlarmFormat             { get; set; } = DefaultFishAlarmFormat;

        // backwards compatibility
        public string AlarmFormat
        {
            set => NodeAlarmFormat = value;
        }
        public bool PrintGigHead
        {
            set => PrintSpearfishInfo = value;
        }


        public int       Version   { get; set; } = 3;
        public ShowNodes ShowNodes { get; set; } = ShowNodes.AllNodes;

        public bool OpenOnStart         { get; set; } = false;
        public bool UseGearChange       { get; set; } = true;
        public bool UseTeleport         { get; set; } = true;
        public bool UseCoordinates      { get; set; } = true;
        public bool WriteCoordinates    { get; set; } = true;
        public bool AlarmsEnabled       { get; set; } = false;
        public bool PrintUptime         { get; set; } = true;
        public bool PrintSpearfishInfo  { get; set; } = true;
        public bool ShowFishTimer       { get; set; } = true;
        public bool FishTimerEdit       { get; set; } = true;
        public bool HideUncaughtFish    { get; set; } = false;
        public bool HideUnavailableFish { get; set; } = false;
        public bool ShowWindowTimers    { get; set; } = true;

        public bool ShowAlreadyCaught { get; set; } = false;
        public bool ShowBigFish       { get; set; } = true;
        public bool ShowSmallFish     { get; set; } = true;
        public bool ShowSpearFish     { get; set; } = true;
        public bool ShowAlwaysUp      { get; set; } = true;
        public byte ShowFishFromPatch { get; set; } = 0;

        public AetherytePreference AetherytePreference { get; set; } = AetherytePreference.Distance;

        public Vector4 AvailableFishColor          { get; set; } = Colors.FishTab.UptimeRunning;
        public Vector4 UpcomingFishColor           { get; set; } = Colors.FishTab.UptimeUpcoming;
        public Vector4 DependentAvailableFishColor { get; set; } = Colors.FishTab.UptimeRunningDependency;
        public Vector4 DependentUpcomingFishColor  { get; set; } = Colors.FishTab.UptimeUpcomingDependency;

        public List<Alarm> Alarms  { get; set; } = new();

        public List<uint>    FixedFish    { get; set; } = new();
        public FishSortOrder FishSortOder { get; set; } = FishSortOrder.EndTime;

        public void Save()
            => Dalamud.PluginInterface.SavePluginConfig(this);

        public static GatherBuddyConfiguration Load()
        {
            if (Dalamud.PluginInterface.GetPluginConfig() is GatherBuddyConfiguration config)
            {
                config.MigrateV2ToV3();
                if (config.IdentifiedItemFormat.Contains("{Location}"))
                    config.IdentifiedItemFormat = DefaultIdentifiedItemFormat;
                return config;
            }

            config = new GatherBuddyConfiguration();
            config.Save();
            return config;
        }
    }
}
