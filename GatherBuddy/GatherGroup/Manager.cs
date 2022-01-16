using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dalamud.Logging;
using GatherBuddy.Classes;
using GatherBuddy.Interfaces;
using GatherBuddy.Time;
using Newtonsoft.Json;

namespace GatherBuddy.GatherGroup;

public class Manager
{
    public const string FileName = "gather_groups.json";

    public SortedList<string, TimedGroup> Groups { get; init; } = new();


    public bool AddGroup(string name, TimedGroup group)
    {
        var lowerName = name.ToLowerInvariant().Trim();
        if (lowerName.Length == 0 || Groups.ContainsKey(lowerName))
            return false;

        Groups.Add(lowerName, group);
        return true;
    }

    public bool ChangeDescription(TimedGroup group, string description)
    {
        if (group.Description == description)
            return false;

        group.Description = description;
        return true;
    }

    public bool RenameGroup(TimedGroup group, string newName)
    {
        if (newName == group.Name)
            return false;

        var newSearchName = newName.ToLowerInvariant().Trim();
        if (newSearchName.Length == 0 || Groups.ContainsKey(newSearchName))
            return false;

        RemoveGroup(group);
        group.Name = newName;
        Groups.Add(newSearchName, group);
        return true;
    }

    public bool RemoveGroup(TimedGroup group)
        => Groups.Remove(group.Name.ToLowerInvariant().Trim());

    public bool ChangeGroupNode(TimedGroup group, int idx, IGatherable? item, int? start, int? end, string? annotation, bool delete)
    {
        if (idx < 0 || group.Nodes.Count > idx)
            return false;

        if (delete)
        {
            if (idx == group.Nodes.Count)
                return false;

            group.Nodes.RemoveAt(idx);
            return true;
        }

        if (group.Nodes.Count == idx && item != null && !delete)
        {
            var newNode = new TimedGroupNode(item)
            {
                EorzeaStartMinute = start == null ? 0 : Math.Clamp(start.Value, 0, RealTime.MinutesPerDay),
                EorzeaEndMinute   = end == null ? 0 : Math.Clamp(end.Value,     0, RealTime.MinutesPerDay),
                Annotation        = annotation ?? string.Empty,
            };
            group.Nodes.Add(newNode);
            return true;
        }

        var changes = false;
        var node    = group.Nodes[idx];
        if (item != null)
        {
            if (!ReferenceEquals(node.Item, item))
                changes = true;
            node.Item = item;
        }

        if (start != null)
        {
            start = Math.Clamp(start.Value, 0, RealTime.MinutesPerDay);
            if (start.Value != node.EorzeaStartMinute)
                changes = true;
            node.EorzeaStartMinute = start.Value;
        }

        if (end != null)
        {
            end = Math.Clamp(end.Value, 0, RealTime.MinutesPerDay);
            if (end.Value != node.EorzeaEndMinute)
                changes = true;
            node.EorzeaEndMinute = end.Value;
        }

        if (annotation != null)
        {
            if (annotation != node.Annotation)
                changes = true;
            node.Annotation = annotation;
        }

        return changes;
    }

    public bool SwapGroupNodes(TimedGroup group, int idx1, int idx2)
    {
        idx1 = Math.Clamp(idx1, 0, group.Nodes.Count - 1);
        idx2 = Math.Clamp(idx2, 0, group.Nodes.Count - 1);
        if (idx1 == idx2)
            return false;

        (group.Nodes[idx1], group.Nodes[idx2]) = (group.Nodes[idx2], group.Nodes[idx1]);
        return true;
    }

    public void Save()
    {
        var file = Utility.Functions.ObtainSaveFile(FileName);
        if (file == null)
            return;

        try
        {
            var text = JsonConvert.SerializeObject(Groups.Values.Select(g => g.ToConfig()), Formatting.Indented);
            File.WriteAllText(file.FullName, text);
        }
        catch (Exception e)
        {
            PluginLog.Error($"Could not write gather groups to file {file.FullName}:\n{e}");
        }
    }

    public bool SetDefaults(bool restore = false)
    {
        var change = false;
        foreach (var cfgGroup in GroupData.DefaultGroups)
        {
            var searchName = cfgGroup.Name.ToLowerInvariant().Trim();
            if (Groups.ContainsKey(searchName))
            {
                if (!restore)
                    continue;

                Groups.Remove(searchName);
            }

            TimedGroup.FromConfig(cfgGroup, out var group);
            Groups.Add(searchName, group);
            change = true;
        }

        return change;
    }


    public static Manager Load()
    {
        var manager = new Manager();
        var file    = Utility.Functions.ObtainSaveFile(FileName);
        if (file is not { Exists: true })
        {
            manager.SetDefaults();
            manager.Save();
            return manager;
        }

        try
        {
            var text    = File.ReadAllText(file.FullName);
            var data    = JsonConvert.DeserializeObject<List<TimedGroup.Config>>(text);
            var changes = false;
            foreach (var config in data)
            {
                if (!TimedGroup.FromConfig(config, out var group))
                {
                    PluginLog.Error($"Invalid items in gather group {group.Name} skipped.");
                    changes = true;
                }

                var searchName = group.Name.ToLowerInvariant().Trim();
                if (searchName.Length == 0)
                {
                    changes = true;
                    PluginLog.Error("Gather group without name found, skipping.");
                    continue;
                }

                if (manager.Groups.ContainsKey(searchName))
                {
                    changes = true;
                    PluginLog.Error($"Multiple gather groups with the same name {searchName} found, skipping later ones.");
                    continue;
                }

                manager.Groups.Add(searchName, group);
            }

            changes |= manager.SetDefaults();
            if (changes)
                manager.Save();
        }
        catch (Exception e)
        {
            PluginLog.Error($"Error loading gather groups:\n{e}");
            manager.Groups.Clear();
            manager.SetDefaults();
            manager.Save();
        }

        return manager;
    }
}
