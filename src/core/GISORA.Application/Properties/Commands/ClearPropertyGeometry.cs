using GISORA.Application.Common;
using GISORA.Domain.Common;
using GISORA.Domain.Properties;

namespace GISORA.Application.Properties.Commands;

public sealed record ClearPropertyGeometryCommand(PropertyId PropertyId, string? Reason);

public sealed class ClearPropertyGeometryHandler(IPropertyRepository repository)
{
    public async Task<Result> HandleAsync(ClearPropertyGeometryCommand command, CancellationToken cancellationToken = default)
    {
        var property = await repository.FindAsync(command.PropertyId, cancellationToken);
        if (property is null)
        {
            return Result.Fail(Error.NotFound("Imovel nao encontrado.", "properties.not-found"));
        }

        try
        {
            property.ClearGeometry(command.Reason ?? string.Empty);
            await repository.UpdateAsync(property, cancellationToken);

            return Result.Ok();
        }
        catch (DomainException exception)
        {
            return Result.Fail(Error.Validation(exception.Message, exception.Code));
        }
    }
}