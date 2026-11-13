using GISORA.Application.Common;
using GISORA.Application.Geospatial;
using GISORA.Domain.Cemetery;
using GISORA.Domain.Common;
using GISORA.Domain.Geospatial;

using CemeteryAggregate = GISORA.Domain.Cemetery.Cemetery;

namespace GISORA.Application.Cemetery.Commands;

public sealed record CreateCemeteryCommand(string Name, Polygon Boundary, Guid? NeighborhoodId);

public sealed class CreateCemeteryHandler(ICemeteryRepository repository, ISpatialOperations spatial)
{
    public async Task<Result<CemeteryId>> HandleAsync(CreateCemeteryCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var name = command.Name.Trim();

            if (await repository.ExistsByNameAsync(name, cancellationToken))
            {
                return Result.Fail<CemeteryId>(
                    Error.Conflict("Ja existe cemiterio com este nome.", "cemetery.duplicated"));
            }

            var area = await spatial.AreaAsync(command.Boundary, cancellationToken);
            var cemetery = CemeteryAggregate.Register(name, command.Boundary, area, command.NeighborhoodId);

            await repository.AddAsync(cemetery, cancellationToken);

            return Result.Ok(cemetery.Id);
        }
        catch (DomainException exception)
        {
            return Result.Fail<CemeteryId>(Error.Validation(exception.Message, exception.Code));
        }
    }
}