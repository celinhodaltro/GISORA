using GISORA.Application.Common;
using GISORA.Domain.Common;
using GISORA.Domain.Geospatial;
using GISORA.Domain.Properties;

namespace GISORA.Application.Properties.Commands;

public sealed record CreatePropertyCommand(
    string Sector,
    string Block,
    string Lot,
    string? Unit,
    Guid StreetId,
    string Number,
    Guid NeighborhoodId,
    decimal LandAreaSquareMeters,
    Geometry? Geometry);

public sealed class CreatePropertyHandler(
    IPropertyRepository repository,
    IPropertyGeometryValidator validator)
{
    public async Task<Result<PropertyId>> HandleAsync(CreatePropertyCommand command, CancellationToken cancellationToken = default)
    {
        if (command.Geometry is { } geometry)
        {
            var validation = await validator.ValidateAsync(geometry, null, cancellationToken);

            if (validation.IsValid is false)
            {
                return Result.Fail<PropertyId>(PropertyGeometryValidationErrors.From(validation));
            }
        }

        try
        {
            var registration = PropertyRegistration.Create(command.Sector, command.Block, command.Lot, command.Unit);

            if (await repository.ExistsByRegistrationAsync(registration, cancellationToken))
            {
                return Result.Fail<PropertyId>(
                    Error.Conflict("Ja existe imovel com esta inscricao.", "properties.registration.duplicated"));
            }

            var property = Property.Register(
                registration,
                new Address(command.StreetId, command.Number, command.NeighborhoodId),
                Area.FromSquareMeters(command.LandAreaSquareMeters),
                command.Geometry);

            await repository.AddAsync(property, cancellationToken);

            return Result.Ok(property.Id);
        }
        catch (DomainException exception)
        {
            return Result.Fail<PropertyId>(Error.Validation(exception.Message, exception.Code));
        }
    }
}