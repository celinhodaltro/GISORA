using System.Globalization;

using GISORA.Application.Common;
using GISORA.Application.Geospatial;
using GISORA.Domain.Properties;

namespace GISORA.Application.Properties;

public sealed class PropertyGeoJsonExportHandler(
    IPropertyQueries queries,
    IPropertyMapQueries mapQueries,
    GeoJsonExportService export)
{
    public async Task<Result<GeoJsonExportFile>> ExportPropertyAsync(
        PropertyId propertyId,
        string targetCrs,
        CancellationToken cancellationToken = default)
    {
        if (CrsCatalog.TryResolve(targetCrs, out var target, out var crsError) is false)
        {
            return Result.Fail<GeoJsonExportFile>(Error.Validation(crsError ?? "CRS de saida invalido.", "geospatial.crs.invalid"));
        }

        var view = await queries.GetGeometryAsync(propertyId, cancellationToken);

        if (view is null)
        {
            return Result.Fail<GeoJsonExportFile>(Error.NotFound("Imovel nao encontrado.", "properties.not-found"));
        }

        if (view.Geometry is null)
        {
            return Result.Fail<GeoJsonExportFile>(
                Error.Validation("O imovel nao possui geometria para exportar.", "properties.geometry-export.empty"));
        }

        var feature = new GeoJsonFeature(
            view.Id.Value.ToString(),
            view.Geometry,
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                ["tipo"] = "imovel",
                ["inscricao"] = view.Registration,
                ["areaLoteM2"] = view.LandAreaSquareMeters.ToString("0.###", CultureInfo.InvariantCulture),
            });

        var content = await export.ExportAsync(
            [feature],
            new GeoJsonExportMetadata("imovel", view.Geometry.SpatialReference, target, DateTimeOffset.UtcNow),
            cancellationToken);

        return Result.Ok(new GeoJsonExportFile(
            $"imovel-{Slug(view.Registration)}.geojson",
            content,
            1,
            target.ToString()));
    }

    public async Task<Result<GeoJsonExportFile>> ExportSetAsync(
        string? registrationContains,
        string targetCrs,
        CancellationToken cancellationToken = default)
    {
        if (CrsCatalog.TryResolve(targetCrs, out var target, out var crsError) is false)
        {
            return Result.Fail<GeoJsonExportFile>(Error.Validation(crsError ?? "CRS de saida invalido.", "geospatial.crs.invalid"));
        }

        var items = await mapQueries.GetMapItemsAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(registrationContains) is false)
        {
            var term = registrationContains.Trim();
            items = items
                .Where(item => item.Registration.Contains(term, StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }

        if (items.Count == 0)
        {
            return Result.Fail<GeoJsonExportFile>(
                Error.Validation("Nenhum imovel com geometria corresponde ao filtro.", "properties.geometry-export.empty"));
        }

        var ordered = items.OrderBy(item => item.Registration, StringComparer.Ordinal).ToArray();
        var features = ordered
            .Select(item => new GeoJsonFeature(
                item.Id.Value.ToString(),
                item.Geometry,
                new Dictionary<string, string?>(StringComparer.Ordinal)
                {
                    ["tipo"] = "imovel",
                    ["inscricao"] = item.Registration,
                    ["areaLoteM2"] = item.LandAreaSquareMeters.ToString("0.###", CultureInfo.InvariantCulture),
                    ["areaConstruidaM2"] = item.BuiltAreaSquareMeters.ToString("0.###", CultureInfo.InvariantCulture),
                }))
            .ToArray();

        var content = await export.ExportAsync(
            features,
            new GeoJsonExportMetadata(
                "imoveis",
                ordered[0].Geometry.SpatialReference,
                target,
                DateTimeOffset.UtcNow),
            cancellationToken);

        return Result.Ok(new GeoJsonExportFile(
            "imoveis.geojson",
            content,
            features.Length,
            target.ToString()));
    }

    private static string Slug(string value)
    {
        var chars = value
            .Trim()
            .ToLowerInvariant()
            .Select(character => char.IsLetterOrDigit(character) ? character : '-')
            .ToArray();
        var slug = new string(chars).Trim('-');

        while (slug.Contains("--", StringComparison.Ordinal))
        {
            slug = slug.Replace("--", "-", StringComparison.Ordinal);
        }

        return slug.Length == 0 ? "geometria" : slug;
    }
}