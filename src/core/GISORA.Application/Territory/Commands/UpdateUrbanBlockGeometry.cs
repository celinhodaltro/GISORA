using GISORA.Application.Common;
using GISORA.Application.Geospatial;
using GISORA.Domain.Common;
using GISORA.Domain.Geospatial;
using GISORA.Domain.Territory;

namespace GISORA.Application.Territory.Commands;

public sealed record UpdateUrbanBlockGeometryCommand(UrbanBlockId UrbanBlockId, Polygon Geometry);

public sealed class UpdateUrbanBlockGeometryHandler(
    IUrbanBlockRepository repository,
    IGeometryValidator geometryValidator)
{
    public async Task<Result> HandleAsync(UpdateUrbanBlockGeometryCommand command, CancellationToken cancellationToken = default)
    {
        var validation = await geometryValidator.ValidateAsync(command.Geometry, cancellationToken);

        if (validation.IsValid is false)
        {
            return Result.Fail(UrbanBlockGeometryErrors.From(validation));
        }

        var urbanBlock = await repository.FindAsync(command.UrbanBlockId, cancellationToken);
        if (urbanBlock is null)
        {
            return Result.Fail(Error.NotFound("Quadra nao encontrada.", "territory.urbanblock.not-found"));
        }

        try
        {
            urbanBlock.ChangeGeometry(command.Geometry);
            await repository.UpdateAsync(urbanBlock, cancellationToken);

            return Result.Ok();
        }
        catch (DomainException exception)
        {
            return Result.Fail(Error.Validation(exception.Message, exception.Code));
        }
    }
}