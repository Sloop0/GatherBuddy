using System;
using System.Linq;
using GatherBuddy.Interfaces;
using GatherBuddy.Utility;
using Lumina.Excel.GeneratedSheets;

namespace GatherBuddy.Classes;

public class FishingSpot : IComparable<FishingSpot>, ILocation
{
    public const uint SpearfishingIdOffset = 1u << 31;

    private readonly object _data;

    public SpearfishingNotebook? SpearfishingSpotData
        => _data as SpearfishingNotebook;

    public Lumina.Excel.GeneratedSheets.FishingSpot? FishingSpotData
        => _data as Lumina.Excel.GeneratedSheets.FishingSpot;

    public Territory  Territory        { get; init; }
    public string     Name             { get; init; }
    public Aetheryte? ClosestAetheryte { get; internal set; }
    public Fish[]     Items            { get; init; }

    public uint Id
        => _data is SpearfishingNotebook sf
            ? sf.RowId
            : ((Lumina.Excel.GeneratedSheets.FishingSpot)_data).RowId;

    public uint UniqueId
        => _data is SpearfishingNotebook sf
            ? sf.RowId | SpearfishingIdOffset
            : ((Lumina.Excel.GeneratedSheets.FishingSpot)_data).RowId;

    public int IntegralXCoord { get; internal set; }
    public int IntegralYCoord { get; internal set; }

    public float XCoord
        => IntegralXCoord is >= 100 and <= 4200 ? IntegralXCoord / 100.0f : 1.0f;

    public float YCoord
        => IntegralYCoord is >= 100 and <= 4200 ? IntegralYCoord / 100.0f : 1.0f;

    public bool Spearfishing
        => _data is SpearfishingNotebook;

    public int CompareTo(FishingSpot? obj)
        => Id.CompareTo(obj?.Id ?? 0);

    private DefaultInfo? _defaultInfo;

    public FishingSpot(GameData data, Lumina.Excel.GeneratedSheets.FishingSpot spot)
    {
        _data     = spot;
        Territory = data.FindOrAddTerritory(spot.TerritoryType.Value) ?? Territory.Invalid;
        Name      = MultiString.ParseSeStringLumina(spot.PlaceName.Value?.Name);

        IntegralXCoord = Maps.NodeToMap(spot.X, Territory.SizeFactor);
        IntegralYCoord = Maps.NodeToMap(spot.Z, Territory.SizeFactor);
        ClosestAetheryte = Territory.Aetherytes.Count > 0
            ? Territory.Aetherytes.ArgMin(a => a.WorldDistance(Territory.Id, IntegralXCoord, IntegralYCoord))
            : null;

        Items = spot.Item.Where(i => i.Row > 0)
            .Select(i => data.Fishes.TryGetValue(i.Row, out var fish) ? fish : null)
            .Where(f => f != null).Cast<Fish>()
            .ToArray();
        foreach (var item in Items)
            item.FishingSpots.Add(this);
    }

    public FishingSpot(GameData data, SpearfishingNotebook spot)
    {
        _data     = spot;
        Territory = data.FindOrAddTerritory(spot.TerritoryType.Value) ?? Territory.Invalid;
        Name      = MultiString.ParseSeStringLumina(spot.PlaceName.Value?.Name);

        IntegralXCoord = Maps.NodeToMap(spot.X, Territory.SizeFactor);
        IntegralYCoord = Maps.NodeToMap(spot.Y, Territory.SizeFactor);
        ClosestAetheryte = Territory.Aetherytes.Count > 0
            ? Territory.Aetherytes.ArgMin(a => a.WorldDistance(Territory.Id, IntegralXCoord, IntegralYCoord))
            : null;

        Items = spot.GatheringPointBase.Value?.Item.Where(i => i > 0)
                .Select(i => data.Fishes.Values.FirstOrDefault(f => f.IsSpearFish && f.FishId == i))
                .Where(f => f != null).Cast<Fish>()
                .ToArray()
         ?? Array.Empty<Fish>();
        foreach (var item in Items)
            item.FishingSpots.Add(this);
    }

    public bool OverwriteWithCustomInfo(Aetheryte? closestAetheryte, float xCoord, float yCoord)
    {
        if (closestAetheryte is { Id: 0 })
            return false;
        if (xCoord is < 1f or > 42f)
            return false;
        if (yCoord is < 1f or > 42f)
            return false;

        _defaultInfo ??= new DefaultInfo()
        {
            ClosestAetheryte = ClosestAetheryte,
            IntegralXCoord   = IntegralXCoord,
            IntegralYCoord   = IntegralYCoord,
        };

        ClosestAetheryte = closestAetheryte;
        IntegralXCoord   = (int)(100f * xCoord);
        IntegralYCoord   = (int)(100f * yCoord);
        return true;
    }

    public void OverwriteWithDefault()
    {
        if (_defaultInfo == null)
            return;

        ClosestAetheryte = _defaultInfo.ClosestAetheryte;
        IntegralXCoord   = _defaultInfo.IntegralXCoord;
        IntegralYCoord   = _defaultInfo.IntegralYCoord;
    }
}
