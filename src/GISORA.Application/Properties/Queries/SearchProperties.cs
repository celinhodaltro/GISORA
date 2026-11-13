using GISORA.Application.Common;

namespace GISORA.Application.Properties.Queries;

public sealed record SearchPropertiesQuery(PropertyFilter Filter);

public sealed class SearchPropertiesHandler(IPropertyQueries queries)
{
    private const int MaxPageSize = 200;

    public async Task<Result<PagedResult<PropertyListItem>>> HandleAsync(
        SearchPropertiesQuery query,
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