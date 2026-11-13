using GISORA.Application.Common;
using GISORA.Domain.Cemetery;
using GISORA.Domain.Common;

namespace GISORA.Application.Cemetery.Commands;

public sealed record CreateCemeterySectorCommand(CemeteryId CemeteryId, string Code);

public sealed class CreateCemeterySectorHandler(ICemeteryRepository cemeteries, ICemeterySectorRepository sectors)
{
    public async Task<Result<CemeterySectorId>> HandleAsync(
        CreateCemeterySectorCommand command,
        CancellationToken cancellationToken = default)
    {
        if (await cemeteries.FindAsync(command.CemeteryId, cancellationToken) is null)
        {
            return Result.Fail<CemeterySectorId>(
                Error.NotFound("Cemiterio nao encontrado.", "cemetery.not-found"));
        }

        try
        {
            var code = command.Code.Trim();

            if (await sectors.ExistsByCodeAsync(command.CemeteryId, code, cancellationToken))
            {
                return Result.Fail<CemeterySectorId>(
                    Error.Conflict("Ja existe setor com este codigo no cemiterio.", "cemetery.sector.duplicated"));
            }

            var sector = CemeterySector.Create(command.CemeteryId, code);
            await sectors.AddAsync(sector, cancellationToken);

            return Result.Ok(sector.Id);
        }
        catch (DomainException exception)
        {
            return Result.Fail<CemeterySectorId>(Error.Validation(exception.Message, exception.Code));
        }
    }
}