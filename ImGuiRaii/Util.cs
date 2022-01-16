using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;

namespace ImGuiOtter;

public static partial class ImGuiUtil
{
    public static bool DrawDisabledButton(string label, Vector2 size, string description, bool disabled)
    {
        using var alpha = ImGuiRaii.PushStyle(ImGuiStyleVar.Alpha, 0.5f, disabled);
        var       ret   = ImGui.Button(label, size);
        alpha.Pop();
        HoverTooltip(description);
        return ret && !disabled;
    }

    public static void DrawTextButton(string text, Vector2 size, uint buttonColor)
    {
        using var color = ImGuiRaii.PushColor(ImGuiCol.Button, buttonColor)
            .Push(ImGuiCol.ButtonActive,  buttonColor)
            .Push(ImGuiCol.ButtonHovered, buttonColor);
        ImGui.Button(text, size);
    }

    public static void DrawTextButton(string text, Vector2 size, uint buttonColor, uint textColor)
    {
        using var color = ImGuiRaii.PushColor(ImGuiCol.Button, buttonColor)
            .Push(ImGuiCol.ButtonActive,  buttonColor)
            .Push(ImGuiCol.ButtonHovered, buttonColor)
            .Push(ImGuiCol.Text,          textColor);
        ImGui.Button(text, size);
    }

    public static void HoverTooltip(string tooltip)
    {
        if (tooltip.Any() && ImGui.IsItemHovered())
            ImGui.SetTooltip(tooltip);
    }

    public static bool Checkbox(string label, string description, bool current, Action<bool> setter)
    {
        var tmp    = current;
        var result = ImGui.Checkbox(label, ref tmp);
        HoverTooltip(description);
        if (!result || tmp == current)
            return false;

        setter(tmp);
        return true;
    }

    public static void DrawTableColumn(string text)
    {
        ImGui.TableNextColumn();
        ImGui.Text(text);
    }

    private static string ColorBytes(uint color)
        => $"#{(byte)(color & 0xFF):X2}{(byte)(color >> 8):X2}{(byte)(color >> 16):X2}{(byte)(color >> 24):X2}";

    public static bool ColorPicker(string label, string tooltip, uint current, Action<uint> setter, uint standard)
    {
        var       ret = false;
        var       old = ImGui.ColorConvertU32ToFloat4(current);
        var       tmp = old;
        using var _   = ImGuiRaii.PushId(label);
        ImGui.BeginGroup();
        if (ImGui.ColorEdit4("", ref tmp, ImGuiColorEditFlags.AlphaPreviewHalf | ImGuiColorEditFlags.NoInputs) && tmp != old)
        {
            setter(ImGui.ColorConvertFloat4ToU32(tmp));
            ret = true;
        }

        ImGui.SameLine();
        using var alpha = ImGuiRaii.PushStyle(ImGuiStyleVar.Alpha, 0.5f, current == standard);
        if (ImGui.Button("Default") && current != standard)
        {
            setter(standard);
            ret = true;
        }

        alpha.Pop();
        HoverTooltip($"Reset this color to {ColorBytes(standard)}.");

        ImGui.SameLine();
        ImGui.Text(label);
        if (tooltip.Any())
            HoverTooltip(tooltip);
        ImGui.EndGroup();

        return ret;
    }

    public static void ClippedDraw<T>(IReadOnlyList<T> data, Action<T> func, float lineHeight)
    {
        ImGuiListClipperPtr clipper;
        unsafe
        {
            clipper = new ImGuiListClipperPtr(ImGuiNative.ImGuiListClipper_ImGuiListClipper());
        }

        clipper.Begin(data.Count, lineHeight);
        while (clipper.Step())
        {
            for (var actualRow = clipper.DisplayStart; actualRow < clipper.DisplayEnd; actualRow++)
            {
                if (actualRow >= data.Count)
                    return;

                if (actualRow < 0)
                    continue;

                func(data[actualRow]);
            }
        }

        clipper.End();
    }

    public static void HoverIcon(ImGuiScene.TextureWrap icon, Vector2 iconSize)
    {
        var size = new Vector2(icon.Width, icon.Height);
        ImGui.Image(icon.ImGuiHandle, iconSize);
        if (iconSize.X > size.X || iconSize.Y > size.Y || !ImGui.IsItemHovered())
            return;

        ImGui.BeginTooltip();
        ImGui.Image(icon.ImGuiHandle, size);
        ImGui.EndTooltip();
    }

    public static uint MiddleColor(uint c1, uint c2)
    {
        var r = ((c1 & 0xFF) + (c2 & 0xFF)) / 2;
        var g = (((c1 >> 8) & 0xFF) + ((c2 >> 8) & 0xFF)) / 2;
        var b = (((c1 >> 16) & 0xFF) + ((c2 >> 16) & 0xFF)) / 2;
        var a = (((c1 >> 24) & 0xFF) + ((c2 >> 24) & 0xFF)) / 2;
        return r | (g << 8) | (b << 16) | (a << 24);
    }

    public static void RightAlign(string text)
    {
        var offset = ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(text).X;
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + offset);
        ImGui.Text(text);
    }

    public static void Center(string text)
    {
        var offset = (ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(text).X) / 2;
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + offset);
        ImGui.Text(text);
    }
}
