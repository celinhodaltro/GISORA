using GISORA.Application.Common;
using GISORA.Application.PublicFacilities;
using GISORA.Domain.PublicFacilities;
using GISORA.Infrastructure.Persistence.Documents;

namespace GISORA.Infrastructure.Persistence.Repositories;

public sealed class LocalStorageFacilityQueries(ILocalStore store) : IFacilityQueries
{
    private const string IndexKey = "facility-index";
    private const string IdsKey = "facility-ids";

    public async Task<PagedResult<FacilityListItem>> SearchAsync(FacilityFilter filter, CancellationToken cancellationToken = default)
    {
        var index = await store.GetAsync<List<FacilityIndexEntry>>(StorageNamespaces.Facilities, IndexKey, cancellationToken) ?? [];
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

        if (filter.NeighborhoodId is { } neighborhood)
        {
            query = query.Where(entry => entry.NeighborhoodId == neighborhood);
        }

        var ordered = query.OrderBy(entry => entry.Name, StringComparer.Ordinal).ToArray();
        var items = ordered
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(entry => new FacilityListItem(
                new PublicFacilityId(entry.Id),
                entry.Name,
                entry.Type,
                entry.NeighborhoodId,
                entry.HasLocation))
            .ToArray();

        return new PagedResult<FacilityListItem>(items, filter.PageNumber, filter.PageSize, ordered.Length);
    }

    public async Task<IReadOnlyList<FacilityMapItem>> GetMapItemsAsync(CancellationToken cancellationToken = default)
    {
        var ids = await store.GetAsync<List<Guid>>(StorageNamespaces.Facilities, IdsKey, cancellationToken) ?? [];
        var items = new List<FacilityMapItem>();

        foreach (var id in ids)
        {
            var document = await store.GetAsync<FacilityDocument>(
                StorageNamespaces.Facilities,
                $"facility-{id}",
                cancellationToken);

            if (document is null || document.Location is null)
            {
                continue;
            }

            var facility = document.ToDomain();
            items.Add(new FacilityMapItem(facility.Id, facility.Name, facility.Type, facility.Location!));
        }

        return items;
    }
}