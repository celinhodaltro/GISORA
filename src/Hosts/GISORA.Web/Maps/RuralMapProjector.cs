using System.Globalization;
using System.Text.Json.Nodes;

using GISORA.Geospatial.Application;
using GISORA.Rural.Application;
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

    public static string UsageLabel(Rural.Domain.RuralUsage usage)
        => usage switch
        {
            Rural.Domain.RuralUsage.Agriculture => "Agricultura",
            Rural.Domain.RuralUsage.Livestock => "Pecuaria",
            Rural.Domain.RuralUsage.Mixed => "Mista",
            Rural.Domain.RuralUsage.Forestry => "Silvicultura",
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