using System.Text.Json.Nodes;

using GISORA.Application.Cemetery;
using GISORA.Application.Geospatial;

namespace GISORA.Presentation.Shared.Maps;

public static class CemeteryMapProjector
{
    public static string CemeteriesToFeatureCollection(
        IReadOnlyList<CemeteryMapItem> items,
        IGeoJsonSerializer serializer)
    {
        var features = items
            .OrderBy(item => item.Name, StringComparer.Ordinal)
            .Select(item => (JsonNode)new JsonObject
            {
                ["type"] = "Feature",
                ["id"] = item.Id.Value.ToString(),
                ["properties"] = new JsonObject
                {
                    ["nome"] = item.Name,
                },
                ["geometry"] = JsonNode.Parse(serializer.Serialize(item.Boundary)),
            })
            .ToArray();

        return new JsonObject
        {
            ["type"] = "FeatureCollection",
            ["features"] = new JsonArray(features),
        }.ToJsonString();
    }

    public static string GravesToFeatureCollection(
        IReadOnlyList<GraveMapItem> items,
        IGeoJsonSerializer serializer)
    {
        var features = items
            .OrderBy(item => item.Code, StringComparer.Ordinal)
            .Select(item => (JsonNode)new JsonObject
            {
                ["type"] = "Feature",
                ["id"] = item.Id.Value.ToString(),
                ["properties"] = new JsonObject
                {
                    ["jazigo"] = item.Code,
                    ["situacao"] = item.IsOccupied ? "Ocupado" : "Livre",
                },
                ["geometry"] = JsonNode.Parse(serializer.Serialize(item.Location)),
            })
            .ToArray();

        return new JsonObject
        {
            ["type"] = "FeatureCollection",
            ["features"] = new JsonArray(features),
        }.ToJsonString();
    }
}