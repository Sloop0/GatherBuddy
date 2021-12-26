using System;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Logging;
using GatherBuddy.Caching;
using GatherBuddy.Gui;
using GatherBuddy.SeFunctions;
using ImGuiNET;

namespace GatherBuddy.FishTimer;

public partial class FishTimer : IDisposable
{
    private const    float   MaxTimerSeconds  = Record.MaxTime;
    private readonly Vector2 _buttonTextAlign = new(0f, 0.1f);
    private readonly Vector2 _itemSpacing     = new(0, 1);

    private readonly FishManager          _fish;
    private readonly Weather.WeatherManager      _weather;
    private readonly CurrentBait          _bait;
    private readonly Parser.FishingParser _parser;
    private readonly EventFramework       _eventFramework;

    private static bool Visible
        => GatherBuddy.Config.ShowFishTimer;

    private static bool EditMode
        => GatherBuddy.Config.FishTimerEdit;


    private Vector2     _rectMin;
    private Vector2     _rectSize;
    private Vector2     _iconSize;
    private float       _lineHeight;
    private FishCache[] _currentFishList = Array.Empty<FishCache>();


    public FishTimer(FishManager fish, Weather.WeatherManager weather)
    {
        _fish           = fish;
        _weather        = weather;
        _bait           = new CurrentBait(Dalamud.SigScanner);
        _parser         = new Parser.FishingParser();
        _eventFramework = new EventFramework(Dalamud.SigScanner);

        Dalamud.PluginInterface.UiBuilder.Draw += Draw;
        _parser.BeganFishing                   += OnBeganFishing;
        _parser.BeganMooching                  += OnMooch;
        _parser.IdentifiedSpot                 += OnIdentification;
        _parser.SomethingBit                   += OnBite;
        _parser.CaughtFish                     += OnCatch;
    }

    public void Dispose()
    {
        Dalamud.PluginInterface.UiBuilder.Draw -= Draw;
        _parser.BeganFishing                   -= OnBeganFishing;
        _parser.BeganMooching                  -= OnMooch;
        _parser.IdentifiedSpot                 -= OnIdentification;
        _parser.SomethingBit                   -= OnBite;
        _parser.CaughtFish                     -= OnCatch;
        _parser.Dispose();
    }

    private bool RecordsValid(Record record)
        => record.SuccessfulBaits.Contains(_currentBait.Id) && record.WithoutSnagging || _snagging;

    private FishCache[] SortedFish()
    {
        if (_currentSpot == null)
            return Array.Empty<FishCache>();

        var enumerable = _currentSpot.Items.Select(f => new FishCache(this, _fish.Fish[f.ItemId]));

        if (GatherBuddy.Config.HideUncaughtFish)
            enumerable = enumerable.Where(f => !f.Uncaught);
        if (GatherBuddy.Config.HideUnavailableFish)
            enumerable = enumerable.Where(f => !f.Unavailable);

        return enumerable.OrderBy(f => f.SortOrder).ToArray();
    }

    private void DrawEditModeTimer(ImDrawListPtr drawList, float rounding)
    {
        ImGui.Text("  Bait");
        ImGui.Text("  Place and Time");
        drawList.AddRect(_rectMin, _rectMin + _rectSize - _itemSpacing, Colors.FishTimer.Line, rounding);
        drawList.AddRectFilled(_rectMin, _rectMin + _rectSize, Colors.FishTimer.EditBackground, rounding);
        ImGui.SetCursorPosY((_rectSize.Y - ImGui.GetTextLineHeightWithSpacing()) / 2);
        DrawCenteredText(_rectSize.X, "FISH");
        ImGui.SetCursorPosY((_rectSize.Y + ImGui.GetTextLineHeightWithSpacing()) / 2);
        DrawCenteredText(_rectSize.X, "TIMER");
        DrawCenteredText(_rectSize.X, "\nDisable \"Edit Fish Timer\"");
        DrawCenteredText(_rectSize.X, "in /GatherBuddy -> Settings");
        DrawCenteredText(_rectSize.X, "to hide this when not fishing.");
    }

    public void Draw()
    {
        const ImGuiWindowFlags editFlags =
            ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar;

        const ImGuiWindowFlags flags = editFlags | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoInputs;

        if (!Visible)
            return;

        if (Dalamud.ClientState.LocalPlayer?.ClassJob == null || !Dalamud.Conditions.Any())
            return;

        var fishing = _start.IsRunning && Dalamud.Conditions[ConditionFlag.Fishing];
        var rodOut  = Dalamud.ClientState.LocalPlayer.ClassJob.Id == 18 && Dalamud.Conditions[ConditionFlag.Gathering];


        if (_eventFramework.FishingState == FishingState.Bite)
        {
            if (_start.IsRunning)
                PluginLog.Verbose("Fish bit after {Milliseconds} milliseconds.", _start.ElapsedMilliseconds);
            _start.Stop();
        }

        if (!fishing)
            _start.Stop();
        if (!rodOut)
        {
            _currentFishList = Array.Empty<FishCache>();
            if (!EditMode)
                return;
        }

        var diff    = _start.ElapsedMilliseconds;
        var diffPos = _rectMin.X + _iconSize.X + 2 + (_rectSize.X - _iconSize.X) * diff / MaxTimerSeconds;

        using var imgui = new ImGuiRaii()
            .PushStyle(ImGuiStyleVar.WindowPadding, Vector2.Zero)
            .PushStyle(ImGuiStyleVar.ItemSpacing,   _itemSpacing);

        _lineHeight = ImGui.GetTextLineHeightWithSpacing() * 1.4f;
        _iconSize   = new Vector2(_lineHeight, _lineHeight);
        var textLines     = 2 * ImGui.GetTextLineHeightWithSpacing();
        var maxListHeight = 10 * (_lineHeight + 1) + textLines;
        var listHeight    = EditMode ? maxListHeight : _currentFishList.Length * (_lineHeight + 1) + textLines;
        var globalScale   = ImGui.GetIO().FontGlobalScale;
        var fivePx        = 5 * globalScale;

        ImGui.SetNextWindowSizeConstraints(new Vector2(225 * globalScale, maxListHeight),
            new Vector2(30000 * globalScale,                              listHeight));

        if (!imgui.BeginWindow("##FishingTimer", EditMode ? editFlags : flags))
            return;

        var drawList = ImGui.GetWindowDrawList();

        _rectSize = new Vector2(ImGui.GetWindowSize().X, maxListHeight);
        _rectMin  = ImGui.GetWindowPos();

        drawList.AddRectFilled(_rectMin, _rectMin + new Vector2(_rectSize.X, textLines), Colors.FishTimer.RectBackground,
            4f * globalScale);
        if (rodOut)
        {
            ImGui.SetCursorPosX(fivePx);
            ImGui.Text(_currentBait.Name);
            ImGui.SetCursorPosX(fivePx);
            ImGui.Text(_currentSpot?.Name ?? "Unknown");
            var displayTimer = (fishing || !_catchHandled) && _start.ElapsedMilliseconds > 0;

            if (displayTimer)
            {
                var secondText = (diff / 1000.0).ToString("00.0");
                ImGui.SameLine(_rectSize.X - ImGui.CalcTextSize(secondText).X - fivePx);
                ImGui.Text(secondText);
            }

            foreach (var fish in _currentFishList)
                fish.Draw(this, drawList);

            if (displayTimer)
                drawList.AddLine(new Vector2(diffPos, _rectMin.Y + textLines),
                    new Vector2(diffPos,              _rectMin.Y + listHeight - 2 * globalScale),
                    Colors.FishTimer.Line, 3 * globalScale);
        }
        else if (EditMode)
        {
            DrawEditModeTimer(drawList, fivePx);
        }
    }

    public static void DrawCenteredText(float xSize, string text)
    {
        var textSize = ImGui.CalcTextSize(text).X;
        ImGui.SetCursorPosX((xSize - textSize) / 2);
        ImGui.Text(text);
    }
}
