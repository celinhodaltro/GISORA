using GISORA.BuildingBlocks.Application;
using GISORA.BuildingBlocks.Domain;
using GISORA.Geospatial.Application;
using GISORA.Geospatial.Domain;
using GISORA.UrbanPlanning.Domain;

namespace GISORA.UrbanPlanning.Application.Commands;

public sealed record CreateZoneCommand(
    string Code,
    string Name,
    ZoneParameters Parameters,
    Polygon Boundary);

public sealed class CreateZoneHandler(IZoneRepository repository, ISpatialOperations spatial)
{
    public async Task<Result<ZoneId>> HandleAsync(CreateZoneCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var code = command.Code.Trim();

            if (await repository.ExistsByCodeAsync(code, cancellationToken))
            {
                return Result.Fail<ZoneId>(
                    Error.Conflict("Ja existe zona com este codigo.", "urban.zone.duplicated"));
            }

            var area = await spatial.AreaAsync(command.Boundary, cancellationToken);
            var zone = Zone.Define(code, command.Name, command.Parameters, command.Boundary, area);

            await repository.AddAsync(zone, cancellationToken);

            return Result.Ok(zone.Id);
        }
        catch (DomainException exception)
        {
            return Result.Fail<ZoneId>(Error.Validation(exception.Message, exception.Code));
        }
    }
}

public sealed record CreateRestrictionCommand(
    RestrictionType Type,
    string Description,
    Polygon Boundary);

public sealed class CreateRestrictionHandler(
    IRestrictionRepository repository,
    IGeometryValidator validator,
    ISpatialOperations spatial)
{
    public async Task<Result<RestrictionId>> HandleAsync(
        CreateRestrictionCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var validation = await validator.ValidateAsync(command.Boundary, cancellationToken);
            if (validation.IsValid is false)
            {
                return Result.Fail<RestrictionId>(
                    Error.Validation(
                        $"Geometria invalida: {string.Join("; ", validation.Errors)}",
                        "urban.restriction.invalid-geometry"));
            }

            var area = await spatial.AreaAsync(command.Boundary, cancellationToken);
            var restriction = Restriction.Define(command.Type, command.Description, command.Boundary, area);

            await repository.AddAsync(restriction, cancellationToken);

            return Result.Ok(restriction.Id);
        }
        catch (DomainException exception)
        {
            return Result.Fail<RestrictionId>(Error.Validation(exception.Message, exception.Code));
        }
    }
}