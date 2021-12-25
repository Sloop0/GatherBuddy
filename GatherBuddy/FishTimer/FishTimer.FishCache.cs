using System;
using System.Numerics;
using GatherBuddy.Caching;
using GatherBuddy.Enums;
using GatherBuddy.Gui;
using GatherBuddy.SeFunctions;
using GatherBuddy.Time;
using ImGuiNET;
using ImGuiScene;

namespace GatherBuddy.FishTimer;

public partial class FishTimer
{
    private readonly struct FishCache
    {
        private readonly TimeInterval _nextUptime;
        private readonly string       _textLine;
        private readonly uint         _color;
        private readonly TextureWrap  _icon;
        private readonly float        _sizeMin;
        private readonly float        _sizeMax;
        public readonly  bool         Valid;
        public readonly  bool         Uncaught;
        public readonly  bool         Unavailable;
        public readonly  ulong        SortOrder;

        public FishCache(FishTimer timer, ExtendedFish fish)
        {
            var bite = fish.Fish.BiteType;

            var catchMin = timer._chum ? fish.Records.EarliestCatchChum : fish.Records.EarliestCatch;
            var catchMax = timer._chum ? fish.Records.LatestCatchChum : fish.Records.LatestCatch;
            _sizeMin  = Math.Max(catchMin / MaxTimerSeconds, 0.0f);
            _sizeMax  = Math.Min(catchMax / MaxTimerSeconds, 1.0f);
            SortOrder = ((ulong)catchMin << 16) | catchMax;
            _icon = fish.Icon;

            Unavailable = false;
            Uncaught    = false;

            if (fish.Predators.Length > 0)
                if (!timer._intuition)
                    Unavailable = true;
            _nextUptime = GatherBuddy.Config.ShowWindowTimers
                ? fish.Fish.NextUptime(timer._weather, timer._currentSpot?.Territory)
                : TimeInterval.Always;
            if (_nextUptime == TimeInterval.Invalid || _nextUptime == TimeInterval.Never)
                _nextUptime = TimeInterval.Always;
            if (SeTime.ServerTime < _nextUptime.Start)
                if (!timer._fishEyes || fish.Fish.IsBigFish || fish.Fish.FishRestrictions.HasFlag(FishRestrictions.Weather))
                    Unavailable = true;
            if (fish.Fish.Snagging == Snagging.Required)
                if (!timer._snagging)
                    Unavailable = true;

            if (!timer.RecordsValid(fish.Records))
                Uncaught = true;

            _color = Colors.FishTimer.FromBiteType(bite, Uncaught);

            _textLine = fish.Name;
            if (Unavailable)
            {
                _color    = Colors.FishTimer.Unavailable;
                SortOrder = ulong.MaxValue;
            }
            else if (Uncaught)
            {
                SortOrder |= 1ul << 33;
            }

            Valid = !Unavailable && _sizeMin > 0.001f && _sizeMax < 0.999f && _sizeMin <= _sizeMax;
        }

        public void Draw(FishTimer timer, ImDrawListPtr ptr)
        {
            var pos    = ImGui.GetCursorPosY();
            var height = ImGui.GetTextLineHeightWithSpacing() * 1.4f;

            var biteMin = Vector2.Zero;
            var biteMax = Vector2.Zero;
            var scale   = ImGui.GetIO().FontGlobalScale;
            var begin   = timer._rectMin + new Vector2(timer._iconSize.X,  0);
            var size    = timer._rectSize - new Vector2(timer._iconSize.X, 0);
            if (Valid)
            {
                biteMin = begin + new Vector2(_sizeMin * size.X - 2 * scale, pos);
                biteMax = begin + new Vector2(_sizeMax * size.X + 2 * scale, pos + height);
                ptr.AddRectFilled(biteMin, biteMax, Colors.FishTimer.Background);
            }

            ImGui.Image(_icon.ImGuiHandle, timer._iconSize);
            ImGui.SameLine();

            var buttonWidth = timer._rectSize.X - timer._iconSize.X;
            using (var _ = new ImGuiRaii()
                       .PushColor(ImGuiCol.Button,        _color)
                       .PushColor(ImGuiCol.ButtonHovered, _color)
                       .PushColor(ImGuiCol.ButtonActive,  _color)
                       .PushStyle(ImGuiStyleVar.ButtonTextAlign, timer._buttonTextAlign))
            {
                ImGui.Button(_textLine, new Vector2(buttonWidth, height));
            }

            if (_nextUptime != TimeInterval.Always)
            {
                var now = TimeStamp.UtcNow;
                var time = _nextUptime.Start < now
                    ? _nextUptime.End - now
                    : _nextUptime.Start - now;
                var s         = Interface.TimeString(time / RealTime.MillisecondsPerSecond, true);
                var t         = ImGui.CalcTextSize(s);
                var width     = t.X;
                var fishWidth = ImGui.CalcTextSize(_textLine).X;
                if (buttonWidth - width - fishWidth >= 5 * ImGui.GetIO().FontGlobalScale)
                {
                    var oldPos = ImGui.GetCursorPos();
                    ImGui.SetCursorScreenPos(begin + new Vector2(size.X - width - 2.5f * ImGui.GetIO().FontGlobalScale, pos));
                    ImGui.AlignTextToFramePadding();
                    ImGui.TextColored(Colors.FishTimer.WindowTimes, s);
                    ImGui.SetCursorPos(oldPos);
                }
            }

            if (!Valid)
                return;

            ptr.AddLine(biteMin, biteMin + new Vector2(0, height), Colors.FishTimer.Separator, 2 * scale);
            ptr.AddLine(biteMax, biteMax - new Vector2(0, height), Colors.FishTimer.Separator, 2 * scale);
        }
    }
}
