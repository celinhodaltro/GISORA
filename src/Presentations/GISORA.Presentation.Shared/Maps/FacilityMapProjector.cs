using System.Globalization;
using System.Text.Json.Nodes;

using GISORA.Application.Geospatial;
using GISORA.Application.PublicFacilities;

namespace GISORA.Presentation.Shared.Maps;

public static class FacilityMapProjector
{
    public static string ToFeatureCollection(IReadOnlyList<FacilityMapItem> items, IGeoJsonSerializer serializer)
    {
        var features = items
            .OrderBy(item => item.Name, StringComparer.Ordinal)
            .Select(item =>
            {
                var symbol = FacilitySymbology.For(item.Type);

                return (JsonNode)new JsonObject
                {
                    ["type"] = "Feature",
                    ["id"] = item.Id.Value.ToString(),
                    ["properties"] = new JsonObject
                    {
                        ["nome"] = item.Name,
                        ["tipo"] = symbol.Label,
                        ["icone"] = symbol.Icon,
                        ["cor"] = symbol.Color,
                    },
                    ["geometry"] = JsonNode.Parse(serializer.Serialize(item.Location)),
                };
            })
            .ToArray();

        return new JsonObject
        {
            ["type"] = "FeatureCollection",
            ["features"] = new JsonArray(features),
        }.ToJsonString();
    }

    public static IReadOnlyList<MapLegendItem> ToLegend(IReadOnlyList<FacilityMapItem> items)
    {
        var symbols = items
            .Select(item => FacilitySymbology.For(item.Type))
            .DistinctBy(symbol => symbol.Type)
            .OrderBy(symbol => symbol.Type)
            .Select(symbol => new MapLegendItem(symbol.Label, symbol.Color, symbol.Icon))
            .ToArray();

        return symbols;
    }

    public static string ToCoordinateText(MapCoordinate coordinate)
        => string.Create(
            CultureInfo.InvariantCulture,
            $"{coordinate.Latitude:F5}, {coordinate.Longitude:F5}");
}