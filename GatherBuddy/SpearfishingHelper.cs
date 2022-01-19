using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Component.GUI;
using GatherBuddy.Enums;
using GatherBuddy.Gui;
using GatherBuddy.SeFunctions;
using ImGuiNET;
using ImGuiOtter;
using Lumina.Excel.GeneratedSheets;
using FishingSpot = GatherBuddy.Classes.FishingSpot;

namespace GatherBuddy;

public class SpearfishingHelper
{
    public readonly Dictionary<uint, FishingSpot> SpearfishingSpots;

    private FishingSpot? _currentSpot;
    private bool         _isOpen;

    public SpearfishingHelper(GameData gameData)
    {
        var points = Dalamud.GameData.GetExcelSheet<GatheringPoint>()!;
        var baseNodes = GatherBuddy.GameData.FishingSpots.Values
            .Where(fs => fs.Spearfishing)
            .ToDictionary(fs => fs.SpearfishingSpotData!.GatheringPointBase.Row, fs => fs);
        SpearfishingSpots = new Dictionary<uint, FishingSpot>(baseNodes.Count);
        foreach (var point in points)
        {
            if (!baseNodes.TryGetValue(point.GatheringPointBase.Row, out var node))
                continue;

            SpearfishingSpots.Add(point.RowId, node);
        }
    }

    private FishingSpot? GetTargetFishingSpot()
    {
        if (Dalamud.Targets.Target == null)
            return null;

        if (Dalamud.Targets.Target.ObjectKind != ObjectKind.GatheringPoint)
            return null;

        var id = Dalamud.Targets.Target.DataId;
        return !SpearfishingSpots.TryGetValue(id, out var spot) ? null : spot;
    }

    private static string Identify(FishingSpot? spot, SpearfishWindow.Info info)
    {
        if (spot == null)
            return "Unknown Fish";

        var fishes = spot.Items.Where(f
                => (f.Speed == info.Speed || f.Speed == SpearfishSpeed.Unknown) && (f.Size == info.Size || f.Size == SpearfishSize.Unknown))
            .ToList();
        return fishes.Count == 0 ? "Unknown Fish" : string.Join("\n", fishes.Select(f => f.Name[GatherBuddy.Language]));
    }

    private static unsafe void DrawFish(FishingSpot? spot, SpearfishWindow.Info info, AtkResNode* node, float yOffset)
    {
        if (!info.Available)
            return;

        var text = Identify(spot, info);
        var size = ImGui.CalcTextSize(text);
        var f1 = new Vector2(node->X + node->ScaleX * node->Width / 2f - size.X / 2,
            node->Y + yOffset + (node->ScaleY * node->Height - size.Y - ImGui.GetStyle().FramePadding.Y) / 2f);
        ImGui.SetCursorPos(f1);
        ImGuiUtil.DrawTextButton(text, Vector2.Zero, 0x40000000);
#if DEBUG
        ImGui.SameLine();
        ImGui.Text($"{info.Size} {info.Speed.ToName()}");
#endif
    }

    private void DrawList(Vector2 pos, Vector2 size)
    {
        if (_currentSpot == null || _currentSpot.Items.Length == 0)
            return;

        ImGuiHelpers.ForceNextWindowMainViewport();
        using var color = ImGuiRaii.PushColor(ImGuiCol.WindowBg, 0x80000000);
        ImGui.SetNextWindowPos(size * Vector2.UnitX + pos);
        if (!ImGui.Begin("SpearfishingHelper2", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.End();
            return;
        }

        using var end = ImGuiRaii.DeferredEnd(ImGui.End);
        var       iconSize   = ImGuiHelpers.ScaledVector2(30, 30);
        foreach (var fish in _currentSpot.Items)
        {
            using var _ = ImGuiRaii.NewGroup();
            ImGui.Image(IconId.FromSpeed(fish.Speed).ImGuiHandle, iconSize);
            ImGui.SameLine();
            ImGui.Image(IconId.FromSize(fish.Size).ImGuiHandle, iconSize);
            ImGui.SameLine();
            ImGui.Image(Icons.DefaultStorage[fish.ItemData.Icon].ImGuiHandle, iconSize);
            ImGui.SameLine();
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + (iconSize.Y - ImGui.GetTextLineHeight())/2);
            ImGui.Text(fish.Name[GatherBuddy.Language]);
        }
    }

    public unsafe void Draw()
    {
        var addon   = (SpearfishWindow*)Dalamud.GameGui.GetAddonByName("SpearFishing", 1);
        var oldOpen = _isOpen;
        _isOpen = addon != null && addon->Base.WindowNode != null;
        if (!_isOpen)
            return;

        if (_isOpen != oldOpen)
            _currentSpot = GetTargetFishingSpot();

        var pos    = new Vector2(addon->Base.X, addon->Base.Y);
        var size = new Vector2(addon->Base.WindowNode->AtkResNode.Width * addon->Base.WindowNode->AtkResNode.ScaleX,
            addon->Base.WindowNode->AtkResNode.Height * addon->Base.WindowNode->AtkResNode.ScaleY);
        ImGui.SetNextWindowSize(size);
        ImGui.SetNextWindowPos(pos);
        ImGuiHelpers.ForceNextWindowMainViewport();
        if (!ImGui.Begin("SpearfishingHelper", ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoInputs))
        {
            ImGui.End();
        }
        else
        {
            using var end = ImGuiRaii.DeferredEnd(ImGui.End);
            DrawFish(_currentSpot, addon->Fish1, addon->Fish1Node, addon->FishLines->Y);
            DrawFish(_currentSpot, addon->Fish2, addon->Fish2Node, addon->FishLines->Y);
            DrawFish(_currentSpot, addon->Fish3, addon->Fish3Node, addon->FishLines->Y);
            var lineStart = pos + new Vector2(size.X / 2, addon->FishLines->Y * addon->FishLines->ScaleY);
            var lineEnd   = lineStart + new Vector2(0,    addon->FishLines->ScaleY * addon->FishLines->Height);
            var list      = ImGui.GetWindowDrawList();
            list.AddLine(lineStart, lineEnd, 0xFF0000C0, 3 * ImGuiHelpers.GlobalScale);
        }

        DrawList(pos, size);
    }
}
