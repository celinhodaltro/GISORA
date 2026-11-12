using GISORA.BuildingBlocks.Application;
using GISORA.BuildingBlocks.Domain;
using GISORA.Geospatial.Domain;
using GISORA.Properties.Domain;

namespace GISORA.Properties.Application.Commands;

public sealed record UpdatePropertyGeometryCommand(PropertyId PropertyId, Polygon Geometry, string? Reason);

public sealed class UpdatePropertyGeometryHandler(IPropertyRepository repository)
{
    public async Task<Result> HandleAsync(UpdatePropertyGeometryCommand command, CancellationToken cancellationToken = default)
    {
        var property = await repository.FindAsync(command.PropertyId, cancellationToken);
        if (property is null)
        {
            return Result.Fail(Error.NotFound("Imovel nao encontrado.", "properties.not-found"));
        }

        try
        {
            property.ChangeGeometry(command.Geometry, command.Reason);
            await repository.UpdateAsync(property, cancellationToken);

            return Result.Ok();
        }
        catch (DomainException exception)
        {
            return Result.Fail(Error.Validation(exception.Message, exception.Code));
        }
    }
}