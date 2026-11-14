using GISORA.Application.Common;
using GISORA.Application.Territory;
using GISORA.Domain.Territory;
using GISORA.Infrastructure.Persistence.Documents;

namespace GISORA.Infrastructure.Persistence.Repositories;

public sealed class LocalStorageUrbanBlockQueries(ILocalStore store) : IUrbanBlockQueries
{
    private const string IndexKey = "urbanblock-index";
    private const string IdsKey = "urbanblock-ids";

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
                entry.FaceCount,
                entry.HasGeometry))
            .ToArray();

        return new PagedResult<UrbanBlockListItem>(items, filter.PageNumber, filter.PageSize, ordered.Length);
    }

    public async Task<IReadOnlyList<UrbanBlockMapItem>> GetMapItemsAsync(CancellationToken cancellationToken = default)
    {
        var ids = await store.GetAsync<List<Guid>>(StorageNamespaces.Territory, IdsKey, cancellationToken) ?? [];
        var items = new List<UrbanBlockMapItem>();

        foreach (var id in ids)
        {
            var document = await store.GetAsync<UrbanBlockDocument>(StorageNamespaces.Territory, $"urbanblock-{id}", cancellationToken);

            if (document is not null)
            {
                items.Add(UrbanBlockMapItem.From(document.ToDomain()));
            }
        }

        return items;
    }
}