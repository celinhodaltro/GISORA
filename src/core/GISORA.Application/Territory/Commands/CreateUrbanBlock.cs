using GISORA.Application.Common;
using GISORA.Application.Geospatial;
using GISORA.Domain.Common;
using GISORA.Domain.Geospatial;
using GISORA.Domain.Territory;

namespace GISORA.Application.Territory.Commands;

public sealed record CreateUrbanBlockCommand(FiscalSectorId FiscalSectorId, string Code, Polygon? Geometry = null);

public sealed class CreateUrbanBlockHandler(
    IUrbanBlockRepository repository,
    IGeometryValidator geometryValidator)
{
    public async Task<Result<UrbanBlockId>> HandleAsync(CreateUrbanBlockCommand command, CancellationToken cancellationToken = default)
    {
        if (command.Geometry is { } geometry)
        {
            var validation = await geometryValidator.ValidateAsync(geometry, cancellationToken);

            if (validation.IsValid is false)
            {
                return Result.Fail<UrbanBlockId>(UrbanBlockGeometryErrors.From(validation));
            }
        }

        try
        {
            if (await repository.ExistsByCodeAsync(command.FiscalSectorId, command.Code, cancellationToken))
            {
                return Result.Fail<UrbanBlockId>(
                    Error.Conflict("Ja existe quadra com este codigo no setor fiscal.", "territory.urbanblock.duplicated"));
            }

            var urbanBlock = UrbanBlock.Create(command.FiscalSectorId, command.Code, command.Geometry);
            await repository.AddAsync(urbanBlock, cancellationToken);

            return Result.Ok(urbanBlock.Id);
        }
        catch (DomainException exception)
        {
            return Result.Fail<UrbanBlockId>(Error.Validation(exception.Message, exception.Code));
        }
    }
}

internal static class UrbanBlockGeometryErrors
{
    public static Error From(GeometryValidationResult validation)
    {
        ArgumentNullException.ThrowIfNull(validation);

        return Error.Validation(
            $"Geometria invalida: {string.Join(" ", validation.Errors)}",
            "territory.geometry.invalid");
    }
}