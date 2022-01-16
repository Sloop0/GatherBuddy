using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Interface;
using ImGuiNET;

namespace ImGuiOtter.Table;

public class HeaderConfigFlags<T, TItem> : HeaderConfig<TItem> where T : struct, Enum
{
    protected T AllFlags = default;

    protected virtual IReadOnlyList<T> Values
        => Enum.GetValues<T>();

    protected virtual string[] Names
        => Enum.GetNames<T>();

    public virtual T FilterValue
        => default;

    protected virtual void SetValue(T value, bool enable)
    { }

    public override bool DrawFilter()
    {
        using var id    = ImGuiRaii.PushId(FilterLabel);
        using var style = ImGuiRaii.PushStyle(ImGuiStyleVar.FrameRounding, 0);
        ImGui.SetNextItemWidth(-Table.ArrowWidth * ImGuiHelpers.GlobalScale);
        using var color = ImGuiRaii.PushColor(ImGuiCol.FrameBg, 0x803030A0, !FilterValue.HasFlag(AllFlags));
        if (!ImGui.BeginCombo(string.Empty, Label, ImGuiComboFlags.NoArrowButton))
            return false;

        color.Pop();

        using var end = ImGuiRaii.DeferredEnd(ImGui.EndCombo);

        var ret = false;
        for (var i = 0; i < Names.Length; ++i)
        {
            var tmp = FilterValue.HasFlag(Values[i]);
            if (!ImGui.Checkbox(Names[i], ref tmp))
                continue;

            SetValue(Values[i], tmp);
            ret = true;
        }

        return ret;
    }
}
