using GISORA.Application.Common;
using GISORA.Domain.Common;
using GISORA.Domain.Territory;

namespace GISORA.Application.Territory.Commands;

public sealed record CreateUrbanBlockCommand(FiscalSectorId FiscalSectorId, string Code);

public sealed class CreateUrbanBlockHandler(IUrbanBlockRepository repository)
{
    public async Task<Result<UrbanBlockId>> HandleAsync(CreateUrbanBlockCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            if (await repository.ExistsByCodeAsync(command.FiscalSectorId, command.Code, cancellationToken))
            {
                return Result.Fail<UrbanBlockId>(
                    Error.Conflict("Ja existe quadra com este codigo no setor fiscal.", "territory.urbanblock.duplicated"));
            }

            var urbanBlock = UrbanBlock.Create(command.FiscalSectorId, command.Code);
            await repository.AddAsync(urbanBlock, cancellationToken);

            return Result.Ok(urbanBlock.Id);
        }
        catch (DomainException exception)
        {
            return Result.Fail<UrbanBlockId>(Error.Validation(exception.Message, exception.Code));
        }
    }
}