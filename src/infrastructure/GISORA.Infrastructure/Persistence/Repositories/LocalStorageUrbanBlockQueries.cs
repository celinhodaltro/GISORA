using GISORA.Application.Common;
using GISORA.Application.Territory;
using GISORA.Domain.Territory;
using GISORA.Infrastructure.Persistence.Documents;

namespace GISORA.Infrastructure.Persistence.Repositories;

public sealed class LocalStorageUrbanBlockQueries(ILocalStore store) : IUrbanBlockQueries
{
    private const string IndexKey = "urbanblock-index";

    public async Task<PagedResult<UrbanBlockListItem>> SearchAsync(UrbanBlockFilter filter, CancellationToken cancellationToken = default)
    {
        var index = await store.GetAsync<List<UrbanBlockIndexEntry>>(StorageNamespaces.Territory, IndexKey, cancellationToken) ?? [];
        var query = index.AsEnumerable();

        if (filter.FiscalSectorId is { } sector)
        {
            query = query.Where(entry => entry.FiscalSectorId == sector.Value);
        }

        if (string.IsNullOrWhiteSpace(filter.CodeContains) is false)
        {
            var term = filter.CodeContains.Trim().ToUpperInvariant();
            query = query.Where(entry => entry.CodeSearch.Contains(term, StringComparison.Ordinal));
        }

        var ordered = query.OrderBy(entry => entry.Code, StringComparer.Ordinal).ToArray();
        var items = ordered
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(entry => new UrbanBlockListItem(
                new UrbanBlockId(entry.Id),
                new FiscalSectorId(entry.FiscalSectorId),
                entry.Code,
                entry.FaceCount))
            .ToArray();

        return new PagedResult<UrbanBlockListItem>(items, filter.PageNumber, filter.PageSize, ordered.Length);
    }
}