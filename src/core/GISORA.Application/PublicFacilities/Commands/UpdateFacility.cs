using GISORA.Application.Common;
using GISORA.Domain.Common;
using GISORA.Domain.Geospatial;
using GISORA.Domain.PublicFacilities;

namespace GISORA.Application.PublicFacilities.Commands;

public sealed record UpdateFacilityCommand(
    PublicFacilityId Id,
    string Name,
    FacilityType Type,
    Guid? NeighborhoodId,
    Point? Location);

public sealed class UpdateFacilityHandler(IPublicFacilityRepository repository)
{
    public async Task<Result> HandleAsync(UpdateFacilityCommand command, CancellationToken cancellationToken = default)
    {
        var facility = await repository.FindAsync(command.Id, cancellationToken);

        if (facility is null)
        {
            return Result.Fail(Error.NotFound("Equipamento nao encontrado.", "facilities.not-found"));
        }

        try
        {
            facility.UpdateDetails(command.Name, command.Type, command.NeighborhoodId);

            if (command.Location is { } location)
            {
                facility.Relocate(location);
            }

            await repository.UpdateAsync(facility, cancellationToken);

            return Result.Ok();
        }
        catch (DomainException exception)
        {
            return Result.Fail(Error.Validation(exception.Message, exception.Code));
        }
    }
}

public sealed record RelocateFacilityCommand(PublicFacilityId Id, Point Location);

public sealed class RelocateFacilityHandler(IPublicFacilityRepository repository)
{
    public async Task<Result> HandleAsync(RelocateFacilityCommand command, CancellationToken cancellationToken = default)
    {
        var facility = await repository.FindAsync(command.Id, cancellationToken);

        if (facility is null)
        {
            return Result.Fail(Error.NotFound("Equipamento nao encontrado.", "facilities.not-found"));
        }

        try
        {
            facility.Relocate(command.Location);
            await repository.UpdateAsync(facility, cancellationToken);

            return Result.Ok();
        }
        catch (DomainException exception)
        {
            return Result.Fail(Error.Validation(exception.Message, exception.Code));
        }
    }
}