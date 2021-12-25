using System;
using GatherBuddy.Classes;
using GatherBuddy.Interfaces;
using Newtonsoft.Json;

namespace GatherBuddy.CustomInfo;

public class LocationDataConverter : JsonConverter<LocationData>
{
    private enum LocationType : byte
    {
        GatheringNode,
        FishingSpot,
    }

    private struct LocationDataStruct
    {
        public int          X;
        public int          Y;
        public LocationType Type;
        public uint         Id;
        public uint         Aetheryte;

        public static LocationDataStruct FromLocationData(LocationData data)
        {
            return new LocationDataStruct
            {
                X = (int)(data.XCoord * 100),
                Y = (int)(data.YCoord * 100),
                Type = data?.Location switch
                {
                    GatheringNode => LocationType.GatheringNode,
                    FishingSpot   => LocationType.FishingSpot,
                    _             => throw new Exception("Invalid type."),
                },
                Id = data.Location switch
                {
                    GatheringNode n => n.BaseId,
                    FishingSpot f   => f.Id,
                    _               => throw new Exception("Invalid type."),
                },
                Aetheryte = data.Aetheryte?.Id ?? 0,
            };
        }
    }

    public override void WriteJson(JsonWriter writer, LocationData? value, JsonSerializer serializer)
    {
        if (value == null)
            writer.WriteToken(JsonToken.Null);
        else
            serializer.Serialize(writer, LocationDataStruct.FromLocationData(value));
    }

    public override LocationData ReadJson(JsonReader reader, Type objectType, LocationData? existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        var s = serializer.Deserialize<LocationDataStruct>(reader);
        var location = s.Type switch
        {
            LocationType.GatheringNode => GatherBuddy.GameData.GatheringNodes.TryGetValue(s.Id, out var node)
                ? (ILocation)node
                : throw new Exception($"Invalid node id {s.Id}."),
            LocationType.FishingSpot => GatherBuddy.GameData.FishingSpots.TryGetValue(s.Id, out var spot)
                ? (ILocation)spot
                : throw new Exception($"Invalid fishing spot id {s.Id}."),
            _ => throw new Exception("Invalid node type."),
        };

        if (existingValue == null)
            existingValue ??= new LocationData(location);
        else
            existingValue.Location = location;

        existingValue.XCoord = s.X / 100f;
        existingValue.YCoord = s.Y / 100f;
        existingValue.Aetheryte = s.Aetheryte == 0
            ? null
            : GatherBuddy.GameData.Aetherytes.TryGetValue(s.Aetheryte, out var aetheryte)
                ? aetheryte
                : throw new Exception($"Invalid aetheryte id {s.Aetheryte}.");
        return existingValue;
    }
}
