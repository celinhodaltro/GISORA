using GISORA.Application.Common;
using GISORA.Application.Geospatial;
using GISORA.Domain.Cemetery;
using GISORA.Domain.Common;
using GISORA.Domain.Geospatial;

namespace GISORA.Application.Cemetery.Commands;

public sealed record CreateGraveCommand(
    CemeteryId CemeteryId,
    CemeterySectorId SectorId,
    string Code,
    Point Location);

public sealed class CreateGraveHandler(
    ICemeteryRepository cemeteries,
    ICemeterySectorRepository sectors,
    IGraveRepository graves,
    ISpatialOperations spatial)
{
    public async Task<Result<GraveId>> HandleAsync(CreateGraveCommand command, CancellationToken cancellationToken = default)
    {
        var cemetery = await cemeteries.FindAsync(command.CemeteryId, cancellationToken);
        if (cemetery is null)
        {
            return Result.Fail<GraveId>(Error.NotFound("Cemiterio nao encontrado.", "cemetery.not-found"));
        }

        var sector = await sectors.FindAsync(command.SectorId, cancellationToken);
        if (sector is null || sector.CemeteryId != command.CemeteryId)
        {
            return Result.Fail<GraveId>(
                Error.Validation("Setor nao pertence ao cemiterio informado.", "cemetery.grave.sector-mismatch"));
        }

        var inside = await spatial.ContainsAsync(cemetery.Boundary, command.Location, cancellationToken);
        if (inside is false)
        {
            return Result.Fail<GraveId>(
                Error.Validation("Jazigo deve estar dentro da area do cemiterio.", "cemetery.grave.outside-cemetery"));
        }

        try
        {
            var code = command.Code.Trim();

            if (await graves.ExistsByCodeAsync(command.SectorId, code, cancellationToken))
            {
                return Result.Fail<GraveId>(
                    Error.Conflict("Ja existe jazigo com este codigo no setor.", "cemetery.grave.duplicated"));
            }

            var grave = Grave.Create(command.CemeteryId, command.SectorId, code, command.Location);
            await graves.AddAsync(grave, cancellationToken);

            return Result.Ok(grave.Id);
        }
        catch (DomainException exception)
        {
            return Result.Fail<GraveId>(Error.Validation(exception.Message, exception.Code));
        }
    }
}