using GISORA.Application.Common;
using GISORA.Domain.Territory;

namespace GISORA.Application.Territory.Queries;

public sealed record GetUrbanBlockQuery(UrbanBlockId UrbanBlockId);

public sealed class GetUrbanBlockHandler(IUrbanBlockRepository repository)
{
    public async Task<Result<UrbanBlockDto>> HandleAsync(GetUrbanBlockQuery query, CancellationToken cancellationToken = default)
    {
        var urbanBlock = await repository.FindAsync(query.UrbanBlockId, cancellationToken);

        return urbanBlock is null
            ? Result.Fail<UrbanBlockDto>(Error.NotFound("Quadra nao encontrada.", "territory.urbanblock.not-found"))
            : Result.Ok(urbanBlock.ToDto());
    }
}