using System.Text.Json.Serialization;

namespace GISORA.Shared.Maps;

public sealed record MapLayer(
    string Id,
    string Name,
    string GeoJson,
    bool Visible = true,
    string? Group = null,
    string? Color = null,
    IReadOnlyList<MapLegendItem>? Legend = null);

public sealed record MapLegendItem(string Label, string Color, string? Icon = null);

public sealed record MapFeature(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("properties")] IReadOnlyDictionary<string, string> Properties);

public readonly record struct MapCoordinate(double Latitude, double Longitude);

public sealed record MapFeatureSelection(string LayerId, MapFeature Feature);