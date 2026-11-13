using System.Globalization;
using System.Text.Json.Nodes;

using GISORA.Application.Geospatial;
using GISORA.Application.Properties;

namespace GISORA.Web.Maps;

public static class PropertyMapProjector
{
    public static string ToFeatureCollection(IReadOnlyList<PropertyMapItem> items, IGeoJsonSerializer serializer)
    {
        var features = items
            .OrderBy(item => item.Registration, StringComparer.Ordinal)
            .Select(item => (JsonNode)new JsonObject
            {
                ["type"] = "Feature",
                ["id"] = item.Id.Value.ToString(),
                ["properties"] = new JsonObject
                {
                    ["inscricao"] = item.Registration,
                    ["areaLote"] = item.LandAreaSquareMeters.ToString("N2", CultureInfo.InvariantCulture),
                    ["areaConstruida"] = item.BuiltAreaSquareMeters.ToString("N2", CultureInfo.InvariantCulture),
                },
                ["geometry"] = JsonNode.Parse(serializer.Serialize(item.Geometry)),
            })
            .ToArray();

        return new JsonObject
        {
            ["type"] = "FeatureCollection",
            ["features"] = new JsonArray(features),
        }.ToJsonString();
    }
}