using System.Text.Json.Nodes;

using GISORA.Geospatial.Application;
using GISORA.Properties.Application;
using GISORA.UrbanPlanning.Application;
using GISORA.UrbanPlanning.Domain;

namespace GISORA.Web.Maps;

public static class RegulationMapProjector
{
    public static string PropertiesToFeatureCollection(
        IReadOnlyList<PropertyMapItem> items,
        IGeoJsonSerializer serializer)
    {
        var features = items
            .OrderBy(item => item.Registration, StringComparer.Ordinal)
            .Select(item => (JsonNode)new JsonObject
            {
                ["type"] = "Feature",
                ["id"] = item.Id.Value.ToString(),
                ["properties"] = new JsonObject
                {
                    ["nome"] = item.Registration,
                },
                ["geometry"] = JsonNode.Parse(serializer.Serialize(item.Geometry)),
            })
            .ToArray();

        return FeatureCollection(features);
    }

    public static string ZonesToFeatureCollection(IReadOnlyList<ZoneMapItem> items, IGeoJsonSerializer serializer)
    {
        var features = items
            .OrderBy(item => item.Code, StringComparer.Ordinal)
            .Select(item => (JsonNode)new JsonObject
            {
                ["type"] = "Feature",
                ["id"] = item.Id.Value.ToString(),
                ["properties"] = new JsonObject
                {
                    ["nome"] = $"{item.Code} - {item.Name}",
                },
                ["geometry"] = JsonNode.Parse(serializer.Serialize(item.Boundary)),
            })
            .ToArray();

        return FeatureCollection(features);
    }

    public static string RestrictionsToFeatureCollection(
        IReadOnlyList<RestrictionMapItem> items,
        IGeoJsonSerializer serializer)
    {
        var features = items
            .OrderBy(item => item.Description, StringComparer.Ordinal)
            .Select(item => (JsonNode)new JsonObject
            {
                ["type"] = "Feature",
                ["id"] = item.Id.Value.ToString(),
                ["properties"] = new JsonObject
                {
                    ["nome"] = item.Description,
                },
                ["geometry"] = JsonNode.Parse(serializer.Serialize(item.Boundary)),
            })
            .ToArray();

        return FeatureCollection(features);
    }

    public static string RestrictionLabel(RestrictionType type)
        => type switch
        {
            RestrictionType.RiskArea => "Area de risco",
            RestrictionType.UrbanPreservationArea => "APP urbana",
            RestrictionType.Setback => "Recuo",
            RestrictionType.HeightLimit => "Gabarito",
            _ => "Restricao",
        };

    private static string FeatureCollection(JsonNode[] features)
        => new JsonObject
        {
            ["type"] = "FeatureCollection",
            ["features"] = new JsonArray(features),
        }.ToJsonString();
}