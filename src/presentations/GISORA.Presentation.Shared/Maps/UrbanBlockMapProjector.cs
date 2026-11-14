using System.Globalization;
using System.Text.Json.Nodes;

using GISORA.Application.Territory;

namespace GISORA.Presentation.Shared.Maps;

public static class UrbanBlockMapProjector
{
    public static string ToBlockFeatureCollection(IReadOnlyList<UrbanBlockMapItem> items)
    {
        var features = items
            .Where(item => item.Rings.Count > 0)
            .OrderBy(item => item.Code, StringComparer.Ordinal)
            .Select(item => (JsonNode)new JsonObject
            {
                ["type"] = "Feature",
                ["id"] = item.Id.Value.ToString(),
                ["properties"] = new JsonObject
                {
                    ["quadra"] = item.Code,
                    ["faces"] = item.Faces.Count.ToString(CultureInfo.InvariantCulture),
                },
                ["geometry"] = new JsonObject
                {
                    ["type"] = "Polygon",
                    ["coordinates"] = ToCoordinates(item.Rings),
                },
            })
            .ToArray();

        return Collection(features);
    }

    public static string ToFaceFeatureCollection(IReadOnlyList<UrbanBlockMapItem> items)
    {
        var features = items
            .OrderBy(item => item.Code, StringComparer.Ordinal)
            .SelectMany(item => item.Faces.Select(face => (JsonNode)new JsonObject
            {
                ["type"] = "Feature",
                ["id"] = face.Id.ToString(),
                ["properties"] = new JsonObject
                {
                    ["quadra"] = item.Code,
                    ["testada"] = face.FrontageMeters.ToString("N2", CultureInfo.InvariantCulture),
                },
                ["geometry"] = new JsonObject
                {
                    ["type"] = "LineString",
                    ["coordinates"] = new JsonArray(face.Coordinates
                        .Select(coordinate => (JsonNode)new JsonArray(coordinate.X, coordinate.Y))
                        .ToArray()),
                },
            }))
            .ToArray();

        return Collection(features);
    }

    private static JsonArray ToCoordinates(IReadOnlyList<IReadOnlyList<CoordinateDto>> rings)
        => new(rings
            .Select(ring => (JsonNode)new JsonArray(ring
                .Select(coordinate => (JsonNode)new JsonArray(coordinate.X, coordinate.Y))
                .ToArray()))
            .ToArray());

    private static string Collection(IReadOnlyList<JsonNode> features)
        => new JsonObject
        {
            ["type"] = "FeatureCollection",
            ["features"] = new JsonArray(features.ToArray()),
        }.ToJsonString();
}