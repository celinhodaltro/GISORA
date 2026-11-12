using System.Globalization;
using System.Text.Json.Nodes;

using GISORA.Geospatial.Application;
using GISORA.PublicFacilities.Application;
using GISORA.Shared.Maps;

namespace GISORA.Web.Maps;

public static class FacilityMapProjector
{
    public static string ToFeatureCollection(IReadOnlyList<FacilityMapItem> items, IGeoJsonSerializer serializer)
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
                    ["tipo"] = item.Type.ToString(),
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

    public static string ToCoordinateText(MapCoordinate coordinate)
        => string.Create(
            CultureInfo.InvariantCulture,
            $"{coordinate.Latitude:F5}, {coordinate.Longitude:F5}");
}