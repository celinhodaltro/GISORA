using System.Text.Json.Serialization;

namespace GISORA.Presentation.Shared.Maps;

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
    [property: JsonPropertyName("properties")] IReadOnlyDictionary<string, string> Properties,
    [property: JsonPropertyName("layerId")] string? LayerId = null);

public sealed record MapPopupRow(string Label, string Value);

public sealed record MapPopupModel(
    string Title,
    IReadOnlyList<MapPopupRow> Rows,
    string? ActionLabel = "Abrir cadastro",
    bool ShowEdit = false,
    string EditLabel = "Editar geometria");

public sealed record MapPopupAction(string LayerId, string FeatureId, string Action);

public readonly record struct MapCoordinate(double Latitude, double Longitude);

public sealed record MapFeatureSelection(string LayerId, MapFeature Feature);

public sealed record MapDrawingMode(string Id, string Label, string Kind);