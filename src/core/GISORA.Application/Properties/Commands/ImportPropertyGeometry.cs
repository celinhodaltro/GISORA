using GISORA.Application.Common;
using GISORA.Application.Geospatial;
using GISORA.Domain.Common;
using GISORA.Domain.Geospatial;
using GISORA.Domain.Properties;

namespace GISORA.Application.Properties.Commands;

public sealed record ImportPropertyGeometryCommand(
    string FileName,
    string GeoJson,
    string? SourceCrs = null,
    string TargetCrs = "EPSG:4326",
    Guid? PropertyId = null);

public sealed record ImportedPropertyGeometry(Guid PropertyId, string Registration);

public sealed record PropertyGeometryImportResult(
    int FeatureCount,
    IReadOnlyList<ImportedPropertyGeometry> Updated,
    IReadOnlyList<string> Skipped);

public sealed class ImportPropertyGeometryHandler(
    IGeoJsonSerializer serializer,
    ICoordinateTransformer transformer,
    IPropertyRepository repository,
    IPropertyQueries queries,
    IPropertyGeometryValidator validator,
    IGeometryImportLog log)
{
    public const string InvalidCrsCode = "geospatial.crs.invalid";

    public const string InvalidGeoJsonCode = "geojson.invalid";

    public const string EmptyImportCode = "properties.geometry-import.empty";

    public async Task<Result<PropertyGeometryImportResult>> HandleAsync(
        ImportPropertyGeometryCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (CrsCatalog.TryResolve(command.TargetCrs, out var target, out var targetError) is false)
        {
            return Result.Fail<PropertyGeometryImportResult>(Error.Validation(targetError ?? "CRS de saida invalido.", InvalidCrsCode));
        }

        var parsed = serializer.TryDeserializeFeatures(command.GeoJson, target);

        if (parsed.Success is false || parsed.Collection is null)
        {
            return Result.Fail<PropertyGeometryImportResult>(
                Error.Validation(parsed.Error ?? "Arquivo GeoJSON invalido.", InvalidGeoJsonCode));
        }

        var collection = parsed.Collection;

        if (collection.Features.Count == 0)
        {
            return Result.Fail<PropertyGeometryImportResult>(
                Error.Validation("O arquivo nao contem feicoes.", InvalidGeoJsonCode));
        }

        if (collection.Features.Count > GeoJsonLimits.MaxFeatures)
        {
            return Result.Fail<PropertyGeometryImportResult>(
                Error.Validation(
                    $"O arquivo excede o limite de {GeoJsonLimits.MaxFeatures} feicoes.",
                    InvalidGeoJsonCode));
        }

        var sourceCrs = command.SourceCrs;

        if (string.IsNullOrWhiteSpace(sourceCrs) && collection.Metadata is { } metadata)
        {
            sourceCrs = metadata.GetValueOrDefault("sourceCrs");
        }

        if (string.IsNullOrWhiteSpace(sourceCrs))
        {
            sourceCrs = target.ToString();
        }

        if (CrsCatalog.TryResolve(sourceCrs, out var source, out var sourceError) is false)
        {
            return Result.Fail<PropertyGeometryImportResult>(Error.Validation(sourceError ?? "CRS de origem invalido.", InvalidCrsCode));
        }

        if (source != target)
        {
            parsed = serializer.TryDeserializeFeatures(command.GeoJson, source);

            if (parsed.Success is false || parsed.Collection is null)
            {
                return Result.Fail<PropertyGeometryImportResult>(
                    Error.Validation(parsed.Error ?? "Arquivo GeoJSON invalido.", InvalidGeoJsonCode));
            }

            collection = parsed.Collection;
        }

        var updated = new List<ImportedPropertyGeometry>();
        var skipped = new List<string>();
        var origin = string.IsNullOrWhiteSpace(command.FileName) ? "importacao" : command.FileName.Trim();
        var importedAt = DateTimeOffset.UtcNow;

        foreach (var feature in collection.Features)
        {
            var property = await FindPropertyAsync(feature, command.PropertyId, cancellationToken);

            if (property is null)
            {
                skipped.Add($"Feicao sem imovel correspondente ({Describe(feature)}).");
                continue;
            }

            try
            {
                var geometry = feature.Geometry.SpatialReference == target
                    ? feature.Geometry
                    : await transformer.TransformAsync(feature.Geometry, target, cancellationToken);

                if (geometry is not (Polygon or MultiPolygon))
                {
                    skipped.Add($"Imovel {property.Registration.Formatted}: tipo {geometry.Type} nao suportado.");
                    continue;
                }

                var validation = await validator.ValidateAsync(geometry, property.Id, cancellationToken);

                if (validation.IsValid is false)
                {
                    skipped.Add($"Imovel {property.Registration.Formatted}: {string.Join(" ", validation.Errors)}");
                    continue;
                }

                property.ChangeGeometry(geometry, $"importacao GeoJSON {origin} ({source} -> {target})");
                await repository.UpdateAsync(property, cancellationToken);

                await log.RecordAsync(
                    new GeometryImportRecord(
                        Guid.CreateVersion7(),
                        property.Id,
                        property.Registration.Formatted,
                        origin,
                        source.ToString(),
                        target.ToString(),
                        collection.Features.Count,
                        importedAt),
                    cancellationToken);

                updated.Add(new ImportedPropertyGeometry(property.Id.Value, property.Registration.Formatted));
            }
            catch (DomainException exception)
            {
                skipped.Add($"Imovel {property.Registration.Formatted}: {exception.Message}");
            }
        }

        if (updated.Count == 0)
        {
            var reason = skipped.Count == 0 ? "Nenhuma feicao pode ser importada." : string.Join(" ", skipped);

            return Result.Fail<PropertyGeometryImportResult>(
                Error.Validation($"Nenhum imovel atualizado. {reason}", EmptyImportCode));
        }

        return Result.Ok(new PropertyGeometryImportResult(collection.Features.Count, updated, skipped));
    }

    private async Task<Property?> FindPropertyAsync(
        GeoJsonFeature feature,
        Guid? forcedPropertyId,
        CancellationToken cancellationToken)
    {
        if (forcedPropertyId is { } forced)
        {
            return await repository.FindAsync(new PropertyId(forced), cancellationToken);
        }

        if (Guid.TryParse(feature.Id, out var featureId))
        {
            var byId = await repository.FindAsync(new PropertyId(featureId), cancellationToken);

            if (byId is not null)
            {
                return byId;
            }
        }

        var registration = Registration(feature);

        if (string.IsNullOrWhiteSpace(registration))
        {
            return null;
        }

        var page = await queries.SearchAsync(
            new PropertyFilter
            {
                RegistrationContains = registration,
                PageNumber = 1,
                PageSize = 100,
            },
            cancellationToken);

        var match = page.Items.FirstOrDefault(item =>
            string.Equals(item.Registration, registration, StringComparison.OrdinalIgnoreCase));

        return match is null ? null : await repository.FindAsync(match.Id, cancellationToken);
    }

    private static string? Registration(GeoJsonFeature feature)
    {
        foreach (var key in new[] { "inscricao", "registration", "id" })
        {
            if (feature.Properties.TryGetValue(key, out var value) && string.IsNullOrWhiteSpace(value) is false)
            {
                return value.Trim();
            }
        }

        return feature.Id;
    }

    private static string Describe(GeoJsonFeature feature)
        => feature.Id ?? Registration(feature) ?? "sem identificador";
}