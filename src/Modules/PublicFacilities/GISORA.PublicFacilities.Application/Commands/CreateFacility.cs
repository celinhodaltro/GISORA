using GISORA.BuildingBlocks.Application;
using GISORA.BuildingBlocks.Domain;
using GISORA.Geospatial.Domain;
using GISORA.PublicFacilities.Domain;

namespace GISORA.PublicFacilities.Application.Commands;

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