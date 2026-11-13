using GISORA.BuildingBlocks.Application;
using GISORA.BuildingBlocks.Domain;
using GISORA.Geospatial.Application;
using GISORA.Geospatial.Domain;
using GISORA.Rural.Domain;

namespace GISORA.Rural.Application.Commands;

public sealed record CreateRuralPropertyCommand(
    string Name,
    Polygon Boundary,
    RuralUsage Usage,
    Guid? MunicipalityId = null,
    string? CarCode = null,
    string? SigefCode = null,
    string? SnciCode = null);

public sealed class CreateRuralPropertyHandler(IRuralPropertyRepository repository, ISpatialOperations spatial)
{
    public async Task<Result<RuralPropertyId>> HandleAsync(
        CreateRuralPropertyCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var name = command.Name.Trim();

            if (await repository.ExistsByNameAsync(name, cancellationToken))
            {
                return Result.Fail<RuralPropertyId>(
                    Error.Conflict("Ja existe propriedade rural com este nome.", "rural.property.duplicated"));
            }

            var area = await spatial.AreaAsync(command.Boundary, cancellationToken);
            var property = RuralProperty.Register(
                name,
                command.Boundary,
                area,
                command.Usage,
                command.MunicipalityId,
                command.CarCode,
                command.SigefCode,
                command.SnciCode);

            await repository.AddAsync(property, cancellationToken);

            return Result.Ok(property.Id);
        }
        catch (DomainException exception)
        {
            return Result.Fail<RuralPropertyId>(Error.Validation(exception.Message, exception.Code));
        }
    }
}

public sealed record ImportRuralPropertyCommand(
    string Name,
    string GeoJson,
    RuralUsage Usage,
    string Crs = "EPSG:4326",
    Guid? MunicipalityId = null,
    string? CarCode = null,
    string? SigefCode = null,
    string? SnciCode = null);

public sealed class ImportRuralPropertyHandler(
    IRuralPropertyRepository repository,
    IGeoJsonSerializer serializer,
    IGeometryValidator validator,
    ISpatialOperations spatial)
{
    public async Task<Result<RuralPropertyId>> HandleAsync(
        ImportRuralPropertyCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var name = command.Name.Trim();

            if (await repository.ExistsByNameAsync(name, cancellationToken))
            {
                return Result.Fail<RuralPropertyId>(
                    Error.Conflict("Ja existe propriedade rural com este nome.", "rural.property.duplicated"));
            }

            var reference = ParseReference(command.Crs);
            var geometry = serializer.Deserialize(command.GeoJson, reference);

            if (geometry is not Polygon boundary)
            {
                return Result.Fail<RuralPropertyId>(
                    Error.Validation("A importacao aceita apenas geometria do tipo Polygon.", "rural.import.geometry"));
            }

            var validation = await validator.ValidateAsync(boundary, cancellationToken);
            if (validation.IsValid is false)
            {
                return Result.Fail<RuralPropertyId>(
                    Error.Validation(
                        $"Geometria invalida: {string.Join("; ", validation.Errors)}",
                        "rural.import.invalid-geometry"));
            }

            var area = await spatial.AreaAsync(boundary, cancellationToken);
            var property = RuralProperty.Register(
                name,
                boundary,
                area,
                command.Usage,
                command.MunicipalityId,
                command.CarCode,
                command.SigefCode,
                command.SnciCode,
                RuralSource.FromGeoJson(reference.ToString()));

            await repository.AddAsync(property, cancellationToken);

            return Result.Ok(property.Id);
        }
        catch (DomainException exception)
        {
            return Result.Fail<RuralPropertyId>(Error.Validation(exception.Message, exception.Code));
        }
    }

    private static SpatialReference ParseReference(string crs)
    {
        var parts = crs.Split(':', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2 || int.TryParse(parts[1], out var code) is false)
        {
            throw new DomainException($"Sistema de referencia invalido: {crs}.", "rural.import.crs");
        }

        return SpatialReference.From(parts[0], code);
    }
}