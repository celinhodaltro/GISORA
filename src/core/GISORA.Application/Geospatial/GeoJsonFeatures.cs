using GISORA.Domain.Geospatial;

namespace GISORA.Application.Geospatial;

public sealed record GeoJsonFeature(
    string? Id,
    Geometry Geometry,
    IReadOnlyDictionary<string, string?> Properties);

public sealed record GeoJsonFeatureSet(
    IReadOnlyList<GeoJsonFeature> Features,
    IReadOnlyDictionary<string, string?>? Metadata = null);

public sealed record GeoJsonParseResult(GeoJsonFeatureSet? Collection, string? Error)
{
    public bool Success => Collection is not null;

    public static GeoJsonParseResult Ok(GeoJsonFeatureSet collection) => new(collection, null);

    public static GeoJsonParseResult Fail(string error) => new(null, error);
}

public static class GeoJsonLimits
{
    public const int MaxBytes = 10 * 1024 * 1024;

    public const int MaxFeatures = 5_000;
}