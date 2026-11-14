using System.Globalization;

using GISORA.Domain.Geospatial;

namespace GISORA.Application.Geospatial;

public sealed record GeoJsonExportMetadata(
    string Origin,
    SpatialReference Source,
    SpatialReference Target,
    DateTimeOffset ExportedAt);

public sealed record GeoJsonExportFile(string FileName, string Content, int FeatureCount, string TargetCrs);

public sealed class GeoJsonExportService(ICoordinateTransformer transformer, IGeoJsonSerializer serializer)
{
    public async Task<string> ExportAsync(
        IReadOnlyList<GeoJsonFeature> features,
        GeoJsonExportMetadata metadata,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(features);
        ArgumentNullException.ThrowIfNull(metadata);

        var exported = new List<GeoJsonFeature>(features.Count);

        foreach (var feature in features)
        {
            var geometry = feature.Geometry.SpatialReference == metadata.Target
                ? feature.Geometry
                : await transformer.TransformAsync(feature.Geometry, metadata.Target, cancellationToken);

            var properties = new Dictionary<string, string?>(feature.Properties, StringComparer.Ordinal)
            {
                ["crs"] = metadata.Target.ToString(),
            };

            exported.Add(feature with { Geometry = geometry, Properties = properties });
        }

        var fileMetadata = new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["origin"] = metadata.Origin,
            ["sourceCrs"] = metadata.Source.ToString(),
            ["targetCrs"] = metadata.Target.ToString(),
            ["exportedAt"] = metadata.ExportedAt.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
            ["featureCount"] = exported.Count.ToString(CultureInfo.InvariantCulture),
        };

        return serializer.SerializeFeatures(new GeoJsonFeatureSet(exported, fileMetadata));
    }
}