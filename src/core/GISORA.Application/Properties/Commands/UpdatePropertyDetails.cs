using GISORA.Application.Common;
using GISORA.Domain.Common;
using GISORA.Domain.Properties;

namespace GISORA.Application.Properties.Commands;

public sealed record UpdatePropertyDetailsCommand(
    PropertyId Id,
    Guid StreetId,
    string Number,
    Guid NeighborhoodId);

public sealed class UpdatePropertyDetailsHandler(IPropertyRepository repository)
{
    public async Task<Result> HandleAsync(
        UpdatePropertyDetailsCommand command,
        CancellationToken cancellationToken = default)
    {
        var property = await repository.FindAsync(command.Id, cancellationToken);

        if (property is null)
        {
            return Result.Fail(Error.NotFound("Imovel nao encontrado.", "properties.not-found"));
        }

        try
        {
            property.ChangeAddress(new Address(command.StreetId, command.Number, command.NeighborhoodId));
            await repository.UpdateAsync(property, cancellationToken);

            return Result.Ok();
        }
        catch (DomainException exception)
        {
            return Result.Fail(Error.Validation(exception.Message, exception.Code));
        }
    }
}