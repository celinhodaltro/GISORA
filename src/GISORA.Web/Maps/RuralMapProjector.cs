using System.Globalization;
using System.Text.Json.Nodes;

using GISORA.Application.Geospatial;
using GISORA.Application.Rural;
namespace GISORA.Web.Maps;

public static class RuralMapProjector
{
    public static string PropertiesToFeatureCollection(
        IReadOnlyList<RuralPropertyMapItem> items,
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

        return FeatureCollection(features);
    }

    public static string PreservationAreasToFeatureCollection(
        IReadOnlyList<PreservationAreaMapItem> items,
        IGeoJsonSerializer serializer)
    {
        var features = items
            .OrderBy(item => item.Label, StringComparer.Ordinal)
            .Select(item => (JsonNode)new JsonObject
            {
                ["type"] = "Feature",
                ["id"] = item.Id.Value.ToString(),
                ["properties"] = new JsonObject
                {
                    ["nome"] = item.Label,
                },
                ["geometry"] = JsonNode.Parse(serializer.Serialize(item.Boundary)),
            })
            .ToArray();

        return FeatureCollection(features);
    }

    public static string UsageLabel(GISORA.Domain.Rural.RuralUsage usage)
        => usage switch
        {
            GISORA.Domain.Rural.RuralUsage.Agriculture => "Agricultura",
            GISORA.Domain.Rural.RuralUsage.Livestock => "Pecuaria",
            GISORA.Domain.Rural.RuralUsage.Mixed => "Mista",
            GISORA.Domain.Rural.RuralUsage.Forestry => "Silvicultura",
            _ => "Outra",
        };

    public static string Hectares(decimal hectares)
        => string.Create(CultureInfo.InvariantCulture, $"{hectares:N2} ha");

    private static string FeatureCollection(JsonNode[] features)
        => new JsonObject
        {
            ["type"] = "FeatureCollection",
            ["features"] = new JsonArray(features),
        }.ToJsonString();
}