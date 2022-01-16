using System.Collections.Generic;
using System.Linq;

namespace GatherBuddy.Alarms;

public partial class AlarmManager
{
    public void ToggleGroup(int idx)
    {
        var group = Alarms[idx];
        if (group.Enabled)
        {
            group.Enabled = false;
            foreach (var alarm in group.Alarms.Where(a => a.Enabled))
                ActiveAlarms.Remove(alarm);
        }
        else
        {
            group.Enabled = true;
            foreach (var alarm in group.Alarms.Where(a => a.Enabled))
                ActiveAlarms.Add(alarm, false);
        }

        Save();
    }

    public void AddGroup(string name)
    {
        var newGroup = new AlarmGroup()
        {
            Name        = name,
            Description = string.Empty,
            Enabled     = false,
            Alarms      = new List<Alarm>(),
        };
        Alarms.Add(newGroup);
        Save();
    }

    public void AddGroup(AlarmGroup group)
    {
        Alarms.Add(group);
        Save();
    }

    public void MoveGroup(int idx1, int idx2)
    {
        if (idx1 == idx2)
            return;

        var group1 = Alarms[idx1];
        Alarms.RemoveAt(idx1);
        Alarms.Insert(idx2, group1);
        Save();
    }

    public void DeleteGroup(int idx)
    {
        var group1 = Alarms[idx];
        if (group1.Enabled)
            foreach (var alarm in group1.Alarms.Where(a => a.Enabled))
                ActiveAlarms.Remove(alarm);
        Alarms.RemoveAt(idx);
        Save();
    }

    public void ChangeGroupName(int idx, string newName)
    {
        var group = Alarms[idx];
        if (group.Name == newName)
            return;

        group.Name = newName;
        Save();
    }

    public void ChangeGroupDescription(int idx, string newDesc)
    {
        var group = Alarms[idx];
        if (group.Description == newDesc)
            return;

        group.Description = newDesc;
        Save();
    }
}
