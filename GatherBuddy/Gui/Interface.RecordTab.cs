using System.IO;
using Dalamud.Interface;
using GatherBuddy.FishTimer;
using ImGuiNET;
using ImGuiOtter;
using ImGuiOtter.Table;
using Lumina.Excel.GeneratedSheets;

namespace GatherBuddy.Gui;

public partial class Interface
{
    private sealed class RecordTable : Table<FishRecord>
    {
        public const string FileNamePopup = "FileNamePopup";

        public RecordTable()
            : base("Fish Records", _plugin.FishRecorder.Records, TextHeight, _catchHeader, _baitHeader, _durationHeader, _castStartHeader,
                _biteTypeHeader, _hookHeader, _spotHeader, _contentIdHeader, _flagHeader)
        { }

        private        int _lastCount;
        private static int _deleteIdx = -1;

        protected override void PreDraw()
        {
            ExtraHeight = ImGui.GetFrameHeightWithSpacing() / ImGuiHelpers.GlobalScale;
            if (_deleteIdx > -1)
            {
                _plugin.FishRecorder.Remove(_deleteIdx);
                _deleteIdx = -1;
            }

            if (_lastCount != Items.Count)
            {
                FilterDirty = true;
                _lastCount  = Items.Count;
            }
        }

        private static readonly ContentIdHeader _contentIdHeader = new() { Label = "Content ID" };
        private static readonly BaitHeader      _baitHeader      = new() { Label = "Bait" };
        private static readonly SpotHeader      _spotHeader      = new() { Label = "Fishing Spot" };
        private static readonly CatchHeader     _catchHeader     = new() { Label = "Caught Fish" };
        private static readonly CastStartHeader _castStartHeader = new() { Label = "TimeStamp" };
        private static readonly BiteTypeHeader  _biteTypeHeader  = new() { Label = "Tug" };
        private static readonly HookHeader      _hookHeader      = new() { Label = "Hookset" };
        private static readonly DurationHeader  _durationHeader  = new() { Label = "Bite" };
        private static readonly FlagHeader      _flagHeader      = new() { Label = "Flags" };

        private class ContentIdHeader : HeaderConfigString<FishRecord>
        {
            public override string ToName(FishRecord item)
                => item.ContentIdHash.ToString("X8");

            public override float Width
                => 75 * ImGuiHelpers.GlobalScale;

            public override int Compare(FishRecord lhs, FishRecord rhs)
                => lhs.ContentIdHash.CompareTo(rhs.ContentIdHash);
        }

        private class BaitHeader : HeaderConfigString<FishRecord>
        {
            public override string ToName(FishRecord item)
                => item.Bait.Name;

            public override float Width
                => 150 * ImGuiHelpers.GlobalScale;
        }

        private class SpotHeader : HeaderConfigString<FishRecord>
        {
            public override string ToName(FishRecord item)
                => item.FishingSpot?.Name ?? "Unknown";

            public override float Width
                => 200 * ImGuiHelpers.GlobalScale;
        }

        private class CatchHeader : HeaderConfigString<FishRecord>
        {
            public CatchHeader()
            {
                Flags |= ImGuiTableColumnFlags.NoHide;
                Flags |= ImGuiTableColumnFlags.NoReorder;
            }

            public override string ToName(FishRecord record)
                => record.Catch?.Name[GatherBuddy.Language] ?? "None";

            public override float Width
                => 200 * ImGuiHelpers.GlobalScale;

            public override void DrawColumn(FishRecord record, int idx)
            {
                base.DrawColumn(record, idx);
                if (ImGui.GetIO().KeyCtrl && ImGui.IsItemClicked(ImGuiMouseButton.Right))
                    _deleteIdx = idx;
                ImGuiUtil.HoverTooltip("Hold Control and right-click to delete...");
            }
        }

        private class CastStartHeader : HeaderConfigString<FishRecord>
        {
            public override string ToName(FishRecord record)
                => (record.TimeStamp.Time / 1000).ToString();

            public override float Width
                => 80 * ImGuiHelpers.GlobalScale;

            public override int Compare(FishRecord lhs, FishRecord rhs)
                => lhs.TimeStamp.CompareTo(rhs.TimeStamp);

            public override void DrawColumn(FishRecord record, int _)
            {
                base.DrawColumn(record, _);
                ImGuiUtil.HoverTooltip(record.TimeStamp.ToString());
            }
        }

        private class BiteTypeHeader : HeaderConfigString<FishRecord>
        {
            public override string ToName(FishRecord item)
                => item.Tug.ToString();

            public override float Width
                => 60 * ImGuiHelpers.GlobalScale;
        }

        private class HookHeader : HeaderConfigString<FishRecord>
        {
            public override string ToName(FishRecord item)
                => item.Hook.ToString();

            public override float Width
                => 85 * ImGuiHelpers.GlobalScale;
        }

        private class DurationHeader : HeaderConfigString<FishRecord>
        {
            public override string ToName(FishRecord record)
                => $"{record.Bite / 1000}.{record.Bite % 1000:D3}";

            public override float Width
                => 50 * ImGuiHelpers.GlobalScale;

            public override void DrawColumn(FishRecord record, int _)
            {
                ImGuiUtil.RightAlign(ToName(record));
            }

            public override int Compare(FishRecord lhs, FishRecord rhs)
                => lhs.Bite.CompareTo(rhs.Bite);
        }

        private class FlagHeader : HeaderConfigString<FishRecord>
        {
            public override string ToName(FishRecord item)
                => item.Flags.ToString();

            public override float Width
                => 0;
        }
    }

    private readonly RecordTable _recordTable;


    private void DrawRecordTab()
    {
        using var id = ImGuiRaii.PushId("Fish Records");
        if (!ImGui.BeginTabItem("Fish Records"))
            return;

        using var end = ImGuiRaii.DeferredEnd(ImGui.EndTabItem);
        _recordTable.Draw();
        if (ImGui.Button("Cleanup"))
        {
            _plugin.FishRecorder.RemoveDuplicates();
            _plugin.FishRecorder.RemoveInvalid();
        }
        ImGuiUtil.HoverTooltip("Delete all entries that were marked as invalid for some reason,\n"
          + "as well as all entries that have a duplicate (with the same content id and timestamp).\n"
          + "Usually, there should be none such entries.\n"
          + "Use at your own risk, no backup will be created automatically.");

        ImGui.SameLine();
        if (ImGui.Button("Copy to Clipboard"))
            ImGui.SetClipboardText(_plugin.FishRecorder.ExportBase64(0, _plugin.FishRecorder.Records.Count));
        ImGuiUtil.HoverTooltip("Export all fish records to your clipboard, to share them with other people.");

        ImGui.SameLine();
        if (ImGui.Button("Import from Clipboard"))
            _plugin.FishRecorder.ImportBase64(ImGui.GetClipboardText());
        ImGuiUtil.HoverTooltip("Import a set of fish records shared with you from your clipboard. Should automatically skip duplicates.");

        ImGui.SameLine();
        if (ImGui.Button("Export JSON"))
            ImGui.OpenPopup(RecordTable.FileNamePopup);

        var name = string.Empty;
        if (ImGuiUtil.OpenNameField(RecordTable.FileNamePopup, ref name) && name.Length > 0)
        {
            var file = new FileInfo(name);
            _plugin.FishRecorder.ExportJson(file);
        }
    }
}
