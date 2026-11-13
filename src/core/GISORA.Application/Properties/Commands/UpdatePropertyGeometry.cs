using GISORA.Application.Common;
using GISORA.Domain.Common;
using GISORA.Domain.Geospatial;
using GISORA.Domain.Properties;

namespace GISORA.Application.Properties.Commands;

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