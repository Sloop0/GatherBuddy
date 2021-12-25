using System;
using GatherBuddy.Utility;
using Lumina.Excel.GeneratedSheets;

namespace GatherBuddy.Structs;

public readonly struct Bait : IComparable<Bait>
{
    public const  uint FishingTackleRow = 30;
    public static Bait Unknown { get; } = new(new Item());

    public readonly Item   Data;
    public readonly string Name;

    public uint Id
        => Data.RowId;

    public Bait(Item data)
    {
        Data = data;
        Name = MultiString.ParseSeStringLumina(data.Name);
    }

    public int CompareTo(Bait other)
        => Id.CompareTo(other.Id);
}
