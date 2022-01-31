using System;
using System.Collections.Generic;
using GatherBuddy.Classes;
using GatherBuddy.Enums;
using GatherBuddy.Structs;
using GatherBuddy.Time;

namespace GatherBuddy.FishTimer;

public struct FishRecord
{
    public const byte Version            = 1;
    public const int  Version1ByteLength = 8 + 8 + 4 + 4 + 2 + 2 + 1 + 1;
    public const int  ByteLength         = Version1ByteLength;

    [Flags]
    public enum Effects : byte
    {
        Snagging      = 0x01,
        Chum          = 0x02,
        Intuition     = 0x04,
        FishEyes      = 0x08,
        IdenticalCast = 0x10,

        Valid = 0x80,
    }

    public ulong       ContentId;
    public TimeStamp   CastStart;
    public Bait        Bait;
    public Fish?       Catch;
    public ushort      Bite;
    public FishingSpot FishingSpot;
    public Effects     Flags;
    public BiteType    BiteType;

    public bool FishEscaped()
        => BiteType != BiteType.Unknown && Catch != null;

    public bool NothingHooked()
        => BiteType == BiteType.Unknown;

    public void ToBytes(IList<byte> bytes, int from)
    {
        if (bytes.Count < from + ByteLength)
            throw new ArgumentException("Not enough storage");

        ToBytes(bytes, from,      ContentId);
        ToBytes(bytes, from + 8,  (ulong)CastStart.Time);
        ToBytes(bytes, from + 16, Bait.Id);
        ToBytes(bytes, from + 20, Catch?.ItemId ?? 0);
        ToBytes(bytes, from + 24, Bite);
        ToBytes(bytes, from + 26, FishingSpot.Id);
        bytes[from + 28] = (byte)Flags;
        bytes[from + 29] = (byte)BiteType;
    }

    private static ushort From2Bytes(ReadOnlySpan<byte> bytes, int from)
        => (ushort)(bytes[from] | (bytes[from + 1] << 8));

    private static uint From4Bytes(ReadOnlySpan<byte> bytes, int from)
        => (uint)(bytes[from] | (bytes[from + 1] << 8) | (bytes[from + 2] << 16) | (bytes[from + 3] << 24));

    private static ulong From8Bytes(ReadOnlySpan<byte> bytes, int from)
        => bytes[from]
          | ((ulong)bytes[from + 1] << 8)
          | ((ulong)bytes[from + 2] << 16)
          | ((ulong)bytes[from + 3] << 24)
          | ((ulong)bytes[from + 4] << 32)
          | ((ulong)bytes[from + 5] << 40)
          | ((ulong)bytes[from + 6] << 48)
          | ((ulong)bytes[from + 7] << 56);

    private static void ToBytes(IList<byte> bytes, int from, ushort value)
    {
        bytes[from]     = (byte)value;
        bytes[from + 1] = (byte)(value >> 8);
    }

    private static void ToBytes(IList<byte> bytes, int from, uint value)
    {
        bytes[from]     = (byte)value;
        bytes[from + 1] = (byte)(value >> 8);
        bytes[from + 2] = (byte)(value >> 16);
        bytes[from + 3] = (byte)(value >> 24);
    }

    private static void ToBytes(IList<byte> bytes, int from, ulong value)
    {
        bytes[from]     = (byte)value;
        bytes[from + 1] = (byte)(value >> 8);
        bytes[from + 2] = (byte)(value >> 16);
        bytes[from + 3] = (byte)(value >> 24);
        bytes[from + 4] = (byte)(value >> 32);
        bytes[from + 5] = (byte)(value >> 40);
        bytes[from + 6] = (byte)(value >> 48);
        bytes[from + 7] = (byte)(value >> 56);
    }

    public static bool FromBytesV1(ReadOnlySpan<byte> bytes, int from, out FishRecord record)
    {
        record = new FishRecord();
        if (bytes.Length < from + 8 + 8 + 4 + 4 + 2 + 2 + 2 + 1)
            return false;

        record.ContentId = From8Bytes(bytes, from);
        record.CastStart = new TimeStamp((long)From8Bytes(bytes, from + 8));
        if (record.CastStart < TimeStamp.Epoch || record.CastStart > GatherBuddy.Time.ServerTime)
            return false;

        var baitId = From4Bytes(bytes, from + 16);
        var bait = GatherBuddy.GameData.Bait.TryGetValue(baitId, out var b) ? b :
            GatherBuddy.GameData.Fishes.TryGetValue(baitId, out var f)      ? new Bait(f.ItemData) : null;
        if (bait == null)
            return false;

        record.Bait = bait;

        var catchId = From4Bytes(bytes, from + 20);
        record.Catch = catchId == 0 ? null : GatherBuddy.GameData.Fishes.TryGetValue(catchId, out var c) ? c : null;
        if (record.Catch == null && catchId != 0)
            return false;

        record.Bite = From2Bytes(bytes, from + 24);
        var fishingSpotId = From2Bytes(bytes, from + 26);
        if (!GatherBuddy.GameData.FishingSpots.TryGetValue(fishingSpotId, out record.FishingSpot!))
            return false;

        record.Flags = (Effects)bytes[28];
        if (!Enum.IsDefined(record.Flags))
            return false;

        record.BiteType = (BiteType)bytes[29];
        if (!Enum.IsDefined(record.BiteType) || record.BiteType == BiteType.Unknown)
            return false;

        return true;
    }
}
