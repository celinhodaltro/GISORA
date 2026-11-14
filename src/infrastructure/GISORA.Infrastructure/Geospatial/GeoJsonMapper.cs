using System.Text;
using System.Text.Json.Nodes;

using GISORA.Application.Geospatial;
using GISORA.Domain.Geospatial;

namespace GISORA.Infrastructure.Geospatial;

internal static class GeoJsonMapper
{
    public static string ToGeoJson(Geometry geometry) => GeometryToJson(geometry).ToJsonString();

    public static Geometry FromGeoJson(string json, SpatialReference reference)
    {
        var node = ParseObject(json);

        return ReadGeometry(node, reference);
    }

    public static string ToFeatureCollection(GeoJsonFeatureSet collection)
    {
        ArgumentNullException.ThrowIfNull(collection);

        var features = new JsonArray(collection.Features
            .Select(feature => (JsonNode)FeatureToJson(feature))
            .ToArray());

        var root = new JsonObject
        {
            ["type"] = "FeatureCollection",
            ["features"] = features,
        };

        if (collection.Metadata is { Count: > 0 } metadata)
        {
            root["gisora"] = MetadataToJson(metadata);
        }

        return root.ToJsonString();
    }

    public static GeoJsonFeatureSet FromFeatures(string json, SpatialReference reference)
    {
        var root = ParseObject(json);
        var type = root["type"]?.GetValue<string>()
                   ?? throw new GeospatialException("GeoJSON sem tipo.");

        if (string.Equals(type, "FeatureCollection", StringComparison.Ordinal))
        {
            var features = root["features"] as JsonArray
                           ?? throw new GeospatialException("FeatureCollection sem lista de feicoes.");

            var parsed = features
                .Select(feature => ReadFeature(feature as JsonObject, reference))
                .ToArray();

            return new GeoJsonFeatureSet(parsed, ReadMetadata(root));
        }

        if (string.Equals(type, "Feature", StringComparison.Ordinal))
        {
            return new GeoJsonFeatureSet([ReadFeature(root, reference)], ReadMetadata(root));
        }

        return new GeoJsonFeatureSet(
            [new GeoJsonFeature(null, ReadGeometry(root, reference), new Dictionary<string, string?>(StringComparer.Ordinal))]);
    }

    private static JsonObject ParseObject(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new GeospatialException("Arquivo GeoJSON vazio.");
        }

        if (Encoding.UTF8.GetByteCount(json) > GeoJsonLimits.MaxBytes)
        {
            throw new GeospatialException(
                $"Arquivo GeoJSON excede o limite de {GeoJsonLimits.MaxBytes / (1024 * 1024)} MB.");
        }

        try
        {
            return JsonNode.Parse(json) as JsonObject
                   ?? throw new GeospatialException("GeoJSON invalido.");
        }
        catch (System.Text.Json.JsonException exception)
        {
            throw new GeospatialException("GeoJSON invalido.", exception);
        }
    }

    private static JsonObject GeometryToJson(Geometry geometry) => geometry switch
    {
        Point point => GeometryObject("Point", Position(point.Coordinate)),
        LineString line => GeometryObject("LineString", Positions(line.Sequence.Coordinates)),
        Polygon polygon => GeometryObject("Polygon", Rings(polygon)),
        MultiPolygon multiPolygon => GeometryObject("MultiPolygon", new JsonArray(
            multiPolygon.Polygons.Select(polygon => (JsonNode)Rings(polygon)).ToArray())),
        _ => throw new GeospatialException($"Geometria nao suportada: {geometry.GetType().Name}."),
    };

    private static Geometry ReadGeometry(JsonObject node, SpatialReference reference)
    {
        var type = node["type"]?.GetValue<string>()
                   ?? throw new GeospatialException("GeoJSON sem tipo.");

        var coordinates = node["coordinates"] as JsonArray
                          ?? throw new GeospatialException("GeoJSON sem coordenadas.");

        return type switch
        {
            "Point" => new Point(ReadPosition(coordinates, reference)),
            "LineString" => new LineString(new CoordinateSequence(ReadPositions(coordinates, reference))),
            "Polygon" => new Polygon(ReadRings(coordinates, reference)),
            "MultiPolygon" => new MultiPolygon(
                coordinates.Select(polygon => new Polygon(ReadRings((JsonArray)polygon!, reference)))),
            _ => throw new GeospatialException($"Tipo GeoJSON nao suportado: {type}."),
        };
    }

    private static GeoJsonFeature ReadFeature(JsonObject? feature, SpatialReference reference)
    {
        if (feature is null)
        {
            throw new GeospatialException("Feicao GeoJSON invalida.");
        }

        var type = feature["type"]?.GetValue<string>();

        if (string.Equals(type, "Feature", StringComparison.Ordinal) is false)
        {
            throw new GeospatialException($"Objeto GeoJSON inesperado na lista de feicoes: {type ?? "sem tipo"}.");
        }

        var geometry = feature["geometry"] as JsonObject
                       ?? throw new GeospatialException("Feicao GeoJSON sem geometria.");

        return new GeoJsonFeature(
            ReadString(feature["id"]),
            ReadGeometry(geometry, reference),
            ReadProperties(feature["properties"] as JsonObject));
    }

    private static JsonObject FeatureToJson(GeoJsonFeature feature)
    {
        var properties = new JsonObject();

        foreach (var entry in feature.Properties)
        {
            if (entry.Value is not null)
            {
                properties[entry.Key] = entry.Value;
            }
        }

        var node = new JsonObject
        {
            ["type"] = "Feature",
            ["properties"] = properties,
            ["geometry"] = GeometryToJson(feature.Geometry),
        };

        if (string.IsNullOrWhiteSpace(feature.Id) is false)
        {
            node["id"] = feature.Id;
        }

        return node;
    }

    private static JsonObject MetadataToJson(IReadOnlyDictionary<string, string?> metadata)
    {
        var node = new JsonObject();

        foreach (var entry in metadata)
        {
            if (entry.Value is not null)
            {
                node[entry.Key] = entry.Value;
            }
        }

        return node;
    }

    private static Dictionary<string, string?>? ReadMetadata(JsonObject root)
    {
        var metadata = new Dictionary<string, string?>(StringComparer.Ordinal);

        if (root["gisora"] is JsonObject gisora)
        {
            foreach (var entry in gisora)
            {
                metadata[entry.Key] = ReadString(entry.Value);
            }
        }

        if (metadata.ContainsKey("sourceCrs") is false
            && root["crs"] is JsonObject crs
            && crs["properties"] is JsonObject properties
            && ReadString(properties["name"]) is { } legacyCrs)
        {
            metadata["sourceCrs"] = legacyCrs;
        }

        return metadata.Count == 0 ? null : metadata;
    }

    private static Dictionary<string, string?> ReadProperties(JsonObject? properties)
    {
        var parsed = new Dictionary<string, string?>(StringComparer.Ordinal);

        if (properties is null)
        {
            return parsed;
        }

        foreach (var entry in properties)
        {
            parsed[entry.Key] = ReadString(entry.Value);
        }

        return parsed;
    }

    private static string? ReadString(JsonNode? node)
    {
        if (node is null)
        {
            return null;
        }

        if (node is JsonValue value && value.TryGetValue<string>(out var text))
        {
            return text;
        }

        return node.ToJsonString();
    }

    private static JsonObject GeometryObject(string type, JsonNode coordinates)
        => new()
        {
            ["type"] = type,
            ["coordinates"] = coordinates,
        };

    private static JsonArray Position(Coordinate coordinate)
        => coordinate.Z is null
            ? new JsonArray { coordinate.X, coordinate.Y }
            : new JsonArray { coordinate.X, coordinate.Y, coordinate.Z.Value };

    private static JsonArray Positions(IEnumerable<Coordinate> coordinates)
        => new(coordinates.Select(coordinate => (JsonNode)Position(coordinate)).ToArray());

    private static JsonArray Rings(Polygon polygon)
        => new(polygon.Rings.Select(ring => (JsonNode)Positions(ring.Coordinates)).ToArray());

    private static Coordinate ReadPosition(JsonArray position, SpatialReference reference)
    {
        if (position.Count < 2)
        {
            throw new GeospatialException("Posicao GeoJSON invalida.");
        }

        var z = position.Count > 2 ? position[2]?.GetValue<double>() : null;

        return new Coordinate(
            position[0]!.GetValue<double>(),
            position[1]!.GetValue<double>(),
            reference,
            z);
    }

    private static IEnumerable<Coordinate> ReadPositions(JsonArray positions, SpatialReference reference)
        => positions.Select(position => ReadPosition((JsonArray)position!, reference));

    private static IEnumerable<CoordinateSequence> ReadRings(JsonArray rings, SpatialReference reference)
        => rings.Select(ring => new CoordinateSequence(ReadPositions((JsonArray)ring!, reference)));
}