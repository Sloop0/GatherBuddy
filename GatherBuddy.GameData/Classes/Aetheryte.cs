using System;
using System.Linq;
using Dalamud.Logging;
using GatherBuddy.Utility;
using Lumina.Excel.GeneratedSheets;
using AetheryteRow = Lumina.Excel.GeneratedSheets.Aetheryte;

namespace GatherBuddy.Classes;

public class Aetheryte : IComparable<Aetheryte>
{
    public AetheryteRow Data      { get; set; }
    public string       Name      { get; set; }
    public Territory    Territory { get; set; }
    public int          XCoord    { get; set; }
    public int          YCoord    { get; set; }

    public short XStream
        => Data.AetherstreamX;

    public short YStream
        => Data.AetherstreamY;

    public uint Id
        => Data.RowId;

    public Aetheryte(GameData gameData, AetheryteRow data)
    {
        Data      = data;
        Name      = MultiString.ParseSeStringLumina(data.AethernetName.Value?.Name);
        Territory = gameData.FindOrAddTerritory(data.Territory.Value) ?? Territory.Invalid;
        var mapMarker = gameData.DataManager.GetExcelSheet<MapMarker>()?.FirstOrDefault(m => m.DataType == 3 && m.DataKey == data.RowId);
        if (mapMarker == null)
            PluginLog.Error($"No Map Marker for Aetheryte {Name} [{data.RowId}].");
        else
        {
            XCoord = Maps.MarkerToMap(mapMarker.X, Territory.SizeFactor);
            XCoord = Maps.MarkerToMap(mapMarker.Y, Territory.SizeFactor);
        }

        Territory.Aetherytes.Add(this);
    }

    public int CompareTo(Aetheryte? rhs)
        => Id.CompareTo(rhs?.Id ?? 0);

    public double WorldDistance(uint mapId, int x, int y)
    {
        if (mapId != Territory.Id)
            return double.PositiveInfinity;

        x -= XCoord;
        y -= YCoord;
        return Math.Sqrt(x * x + y * y);
    }

    public double AetherDistance(int x, int y)
    {
        x -= XStream;
        y -= YStream;
        return Math.Sqrt(x * x + y * y);
    }

    public double AetherDistance(Aetheryte rhs)
        => AetherDistance(rhs.XStream, rhs.YStream);

    public override string ToString()
        => $"{Name} - {Territory!.Name}-{XCoord / 100.0:F2}:{YCoord / 100.0:F2}";
}
