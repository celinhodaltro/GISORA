using System.Text.Json.Nodes;

using GISORA.Geospatial.Domain;

namespace GISORA.Infrastructure.Geospatial;

internal static class GeoJsonMapper
{
    public static string ToGeoJson(Geometry geometry) => geometry switch
    {
        Point point => Serialize("Point", Position(point.Coordinate)),
        LineString line => Serialize("LineString", Positions(line.Sequence.Coordinates)),
        Polygon polygon => Serialize("Polygon", Rings(polygon)),
        MultiPolygon multiPolygon => Serialize("MultiPolygon", new JsonArray(
            multiPolygon.Polygons.Select(polygon => (JsonNode)Rings(polygon)).ToArray())),
        _ => throw new GeospatialException($"Geometria nao suportada: {geometry.GetType().Name}."),
    };

    public static Geometry FromGeoJson(string json, SpatialReference reference)
    {
        var node = JsonNode.Parse(json) as JsonObject
                   ?? throw new GeospatialException("GeoJSON invalido.");

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

    private static string Serialize(string type, JsonNode coordinates)
        => new JsonObject
        {
            ["type"] = type,
            ["coordinates"] = coordinates,
        }.ToJsonString();

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