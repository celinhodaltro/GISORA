using GISORA.BuildingBlocks.Application;
using GISORA.BuildingBlocks.Domain;
using GISORA.Properties.Domain;

namespace GISORA.Properties.Application.Commands;

public sealed record AddOwnershipCommand(PropertyId PropertyId, TaxpayerId TaxpayerId, decimal Percentage, DateOnly Since);

public sealed class AddOwnershipHandler(IPropertyRepository repository, ITaxpayerRepository taxpayers)
{
    public async Task<Result<Guid>> HandleAsync(AddOwnershipCommand command, CancellationToken cancellationToken = default)
    {
        var property = await repository.FindAsync(command.PropertyId, cancellationToken);
        if (property is null)
        {
            return Result.Fail<Guid>(Error.NotFound("Imovel nao encontrado.", "properties.not-found"));
        }

        if (await taxpayers.FindAsync(command.TaxpayerId, cancellationToken) is null)
        {
            return Result.Fail<Guid>(Error.Validation("Contribuinte nao encontrado.", "properties.taxpayer.not-found"));
        }

        try
        {
            var ownership = property.AddOwnership(
                command.TaxpayerId,
                OwnershipShare.FromPercentage(command.Percentage),
                command.Since);

            await repository.UpdateAsync(property, cancellationToken);

            return Result.Ok(ownership.Id);
        }
        catch (DomainException exception)
        {
            return Result.Fail<Guid>(Error.Validation(exception.Message, exception.Code));
        }
    }
}