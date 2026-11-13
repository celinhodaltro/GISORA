using GISORA.Application.Common;
using GISORA.Application.Geospatial;
using GISORA.Domain.Common;
using GISORA.Domain.Geospatial;
using GISORA.Domain.Rural;

namespace GISORA.Application.Rural.Commands;

public sealed record AddPreservationAreaCommand(RuralPropertyId PropertyId, string Label, Polygon Boundary);

public sealed class AddPreservationAreaHandler(IRuralPropertyRepository repository, ISpatialOperations spatial)
{
    public async Task<Result<PreservationAreaId>> HandleAsync(
        AddPreservationAreaCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var property = await repository.FindAsync(command.PropertyId, cancellationToken);

            if (property is null)
            {
                return Result.Fail<PreservationAreaId>(
                    Error.NotFound("Propriedade rural nao encontrada.", "rural.property.not-found"));
            }

            var area = await spatial.AreaAsync(command.Boundary, cancellationToken);

            if (property.LegalReserve is { } reserve
                && await spatial.IntersectsAsync(reserve.Boundary, command.Boundary, cancellationToken))
            {
                var intersection = await spatial.IntersectionAsync(reserve.Boundary, command.Boundary, cancellationToken);
                var overlap = intersection is null
                    ? Area.Zero
                    : await spatial.AreaAsync(intersection, cancellationToken);

                if (overlap.SquareMeters > 0m)
                {
                    return Result.Fail<PreservationAreaId>(
                        Error.Validation(
                            "APP sobrepoe a reserva legal; registre a sobreposicao antes de salvar.",
                            "rural.app.legal-reserve-overlap"));
                }
            }

            var preservationArea = PreservationArea.Create(command.Label, command.Boundary, area);

            property.AddPreservationArea(preservationArea);
            await repository.UpdateAsync(property, cancellationToken);

            return Result.Ok(preservationArea.Id);
        }
        catch (DomainException exception)
        {
            return Result.Fail<PreservationAreaId>(Error.Validation(exception.Message, exception.Code));
        }
    }
}

public sealed record DefineLegalReserveCommand(RuralPropertyId PropertyId, Polygon Boundary);

public sealed class DefineLegalReserveHandler(IRuralPropertyRepository repository, ISpatialOperations spatial)
{
    public async Task<Result> HandleAsync(DefineLegalReserveCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var property = await repository.FindAsync(command.PropertyId, cancellationToken);

            if (property is null)
            {
                return Result.Fail(Error.NotFound("Propriedade rural nao encontrada.", "rural.property.not-found"));
            }

            var area = await spatial.AreaAsync(command.Boundary, cancellationToken);
            property.DefineLegalReserve(command.Boundary, area);

            await repository.UpdateAsync(property, cancellationToken);

            return Result.Ok();
        }
        catch (DomainException exception)
        {
            return Result.Fail(Error.Validation(exception.Message, exception.Code));
        }
    }
}