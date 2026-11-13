using GISORA.Application.Common;
using GISORA.Domain.Common;
using GISORA.Domain.Geospatial;
using GISORA.Domain.PublicFacilities;

namespace GISORA.Application.PublicFacilities.Commands;

public sealed record CreateFacilityCommand(string Name, FacilityType Type, Guid? NeighborhoodId, Point? Location);

public sealed class CreateFacilityHandler(IPublicFacilityRepository repository)
{
    public async Task<Result<PublicFacilityId>> HandleAsync(CreateFacilityCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var name = command.Name.Trim();

            if (await repository.ExistsByNameAsync(name, command.NeighborhoodId, cancellationToken))
            {
                return Result.Fail<PublicFacilityId>(
                    Error.Conflict("Ja existe equipamento com este nome no bairro.", "facilities.duplicated"));
            }

            var facility = PublicFacility.Register(name, command.Type, command.NeighborhoodId, command.Location);
            await repository.AddAsync(facility, cancellationToken);

            return Result.Ok(facility.Id);
        }
        catch (DomainException exception)
        {
            return Result.Fail<PublicFacilityId>(Error.Validation(exception.Message, exception.Code));
        }
    }
}