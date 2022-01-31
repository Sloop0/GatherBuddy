using System;
using GatherBuddy.Interfaces;

namespace GatherBuddy.GatherHelper;

public partial class GatherWindowManager
{
    public void AddPreset(GatherWindowPreset preset)
    {
        Presets.Add(preset);
        Save();
        if (preset.HasItems())
            SetActiveItems();
    }

    public void DeletePreset(int idx)
    {
        if (idx < 0 || idx >= Presets.Count)
            return;

        var enabled = Presets[idx].HasItems();
        Presets.RemoveAt(idx);
        Save();
        if (enabled)
            SetActiveItems();
    }

    public void SwapPreset(int idx1, int idx2)
    {
        idx1                           = Math.Clamp(idx1, 0, Presets.Count);
        idx2                           = Math.Clamp(idx2, 0, Presets.Count);
        if (idx1 == idx2)
            return;

        (Presets[idx1], Presets[idx2]) = (Presets[idx2], Presets[idx1]);
        Save();
        if (Presets[idx1].HasItems() || Presets[idx2].HasItems())
            SetActiveItems();
    }

    public void ChangeName(GatherWindowPreset preset, string newName)
    {
        if (newName == preset.Name)
            return;

        preset.Name = newName;
        Save();
    }

    public void ChangeDescription(GatherWindowPreset preset, string newDescription)
    {
        if (newDescription == preset.Description)
            return;

        preset.Description = newDescription;
        Save();
    }

    public void TogglePreset(GatherWindowPreset preset)
    {
        preset.Enabled = !preset.Enabled;
        Save();
        if (preset.Items.Count > 0)
            SetActiveItems();
    }

    public void AddItem(GatherWindowPreset preset, IGatherable item)
    {
        if (!preset.Add(item))
            return;

        Save();
        if (preset.Enabled)
            SetActiveItems();
    }

    public void RemoveItem(GatherWindowPreset preset, int idx)
    {
        if (idx < 0 || idx >= preset.Items.Count)
            return;

        preset.Items.RemoveAt(idx);
        Save();
        if (preset.Enabled)
            SetActiveItems();
    }

    public void ChangeItem(GatherWindowPreset preset, IGatherable item, int idx)
    {
        if (idx < 0 || idx >= preset.Items.Count)
            return;

        if (ReferenceEquals(preset.Items[idx], item))
            return;

        preset.Items[idx] = item;
        Save();
        if (preset.Enabled)
            SetActiveItems();
    }

    public void MoveItems(GatherWindowPreset preset, int idx1, int idx2)
    {
        idx1                           = Math.Clamp(idx1, 0, Presets.Count);
        idx2                           = Math.Clamp(idx2, 0, Presets.Count);
        if (idx1 == idx2)
            return;

        (preset.Items[idx1], preset.Items[idx2]) = (preset.Items[idx2], preset.Items[idx1]);
        Save();
        if (preset.Enabled)
            SetActiveItems();
    }
}
