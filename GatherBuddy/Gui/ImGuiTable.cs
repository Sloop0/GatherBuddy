using System;
using System.Collections.Generic;
using ImGuiNET;

namespace GatherBuddy.Gui;

public static class ImGuiTable
{
    public static void DrawTable<T>(string label, IEnumerable<T> data, Action<T> drawRow, ImGuiTableFlags flags = ImGuiTableFlags.None,
        params string[] columnTitles)
    {
        if (columnTitles.Length == 0)
            return;

        if (!ImGui.BeginTable(label, columnTitles.Length, flags))
            return;

        try
        {
            foreach (var title in columnTitles)
            {
                ImGui.TableNextColumn();
                ImGui.TableHeader(title);
            }

            foreach (var datum in data)
            {
                ImGui.TableNextRow();
                drawRow(datum);
            }
        }
        finally
        {
            ImGui.EndTable();
        }
    }

    public static void DrawTabbedTable<T>(string label, IEnumerable<T> data, Action<T> drawRow, ImGuiTableFlags flags = ImGuiTableFlags.None,
        params string[] columnTitles)
    {
        if (ImGui.CollapsingHeader(label))
            DrawTable($"{label}##Table", data, drawRow, flags, columnTitles);
    }
}
