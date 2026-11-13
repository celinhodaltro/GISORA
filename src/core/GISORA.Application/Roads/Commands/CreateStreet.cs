using GISORA.Application.Common;
using GISORA.Domain.Common;
using GISORA.Domain.Roads;

namespace GISORA.Application.Roads.Commands;

public sealed record CreateStreetCommand(string Name, StreetType Type, Guid? NeighborhoodId);

public sealed class CreateStreetHandler(IStreetRepository repository)
{
    public async Task<Result<StreetId>> HandleAsync(CreateStreetCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedName = command.Name.Trim();

            if (await repository.ExistsByNameAsync(normalizedName, command.NeighborhoodId, cancellationToken))
            {
                return Result.Fail<StreetId>(
                    Error.Conflict("Ja existe logradouro com este nome.", "roads.street.duplicated"));
            }

            var street = Street.Create(normalizedName, command.Type, command.NeighborhoodId);
            await repository.AddAsync(street, cancellationToken);

            return Result.Ok(street.Id);
        }
        catch (DomainException exception)
        {
            return Result.Fail<StreetId>(Error.Validation(exception.Message, exception.Code));
        }
    }
}