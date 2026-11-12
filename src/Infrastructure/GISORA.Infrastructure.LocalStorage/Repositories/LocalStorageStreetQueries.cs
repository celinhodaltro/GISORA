using GISORA.BuildingBlocks.Application;
using GISORA.Infrastructure.LocalStorage.Persistence;
using GISORA.Roads.Application;
using GISORA.Roads.Domain;

namespace GISORA.Infrastructure.LocalStorage.Repositories;

public sealed class LocalStorageStreetQueries(ILocalStore store) : IStreetQueries
{
    private const string IndexKey = "street-index";

    public async Task<PagedResult<StreetListItem>> SearchAsync(StreetFilter filter, CancellationToken cancellationToken = default)
    {
        var index = await store.GetAsync<List<StreetIndexEntry>>(StorageNamespaces.Roads, IndexKey, cancellationToken) ?? [];
        var query = index.AsEnumerable();

        if (string.IsNullOrWhiteSpace(filter.NameContains) is false)
        {
            var term = filter.NameContains.Trim().ToUpperInvariant();
            query = query.Where(entry => entry.NameSearch.Contains(term, StringComparison.Ordinal));
        }

        if (filter.Type is { } type)
        {
            query = query.Where(entry => entry.Type == type);
        }

        var ordered = query.OrderBy(entry => entry.Name, StringComparer.Ordinal).ToArray();
        var items = ordered
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(entry => new StreetListItem(new StreetId(entry.Id), entry.Name, entry.Type, entry.NeighborhoodId))
            .ToArray();

        return new PagedResult<StreetListItem>(items, filter.PageNumber, filter.PageSize, ordered.Length);
    }
}