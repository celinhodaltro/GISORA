using GISORA.BuildingBlocks.Application;

namespace GISORA.Territory.Application.Queries;

public sealed record SearchUrbanBlocksQuery(UrbanBlockFilter Filter);

public sealed class SearchUrbanBlocksHandler(IUrbanBlockQueries queries)
{
    private const int MaxPageSize = 200;

    public async Task<Result<PagedResult<UrbanBlockListItem>>> HandleAsync(
        SearchUrbanBlocksQuery query,
        CancellationToken cancellationToken = default)
    {
        var filter = query.Filter with
        {
            PageNumber = Math.Max(1, query.Filter.PageNumber),
            PageSize = Math.Clamp(query.Filter.PageSize, 1, MaxPageSize),
        };

        var page = await queries.SearchAsync(filter, cancellationToken);

        return Result.Ok(page);
    }
}