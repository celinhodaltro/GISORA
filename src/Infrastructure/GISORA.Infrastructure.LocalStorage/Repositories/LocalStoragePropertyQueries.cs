using GISORA.BuildingBlocks.Application;
using GISORA.Infrastructure.LocalStorage.Persistence;
using GISORA.Properties.Application;
using GISORA.Properties.Domain;

namespace GISORA.Infrastructure.LocalStorage.Repositories;

public sealed class LocalStoragePropertyQueries(ILocalStore store) : IPropertyQueries
{
    private const string IndexKey = "property-index";

    public async Task<PagedResult<PropertyListItem>> SearchAsync(PropertyFilter filter, CancellationToken cancellationToken = default)
    {
        var index = await store.GetAsync<List<PropertyIndexEntry>>(StorageNamespaces.Properties, IndexKey, cancellationToken) ?? [];
        var query = index.AsEnumerable();

        if (string.IsNullOrWhiteSpace(filter.RegistrationContains) is false)
        {
            var term = filter.RegistrationContains.Trim().ToUpperInvariant();
            query = query.Where(entry => entry.RegistrationSearch.Contains(term, StringComparison.Ordinal));
        }

        if (filter.NeighborhoodId is { } neighborhood)
        {
            query = query.Where(entry => entry.NeighborhoodId == neighborhood);
        }

        var ordered = query.OrderBy(entry => entry.Registration, StringComparer.Ordinal).ToArray();
        var items = ordered
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(entry => new PropertyListItem(
                new PropertyId(entry.Id),
                entry.Registration,
                entry.Number,
                entry.LandAreaSquareMeters,
                entry.BuiltAreaSquareMeters,
                entry.BuildingCount))
            .ToArray();

        return new PagedResult<PropertyListItem>(items, filter.PageNumber, filter.PageSize, ordered.Length);
    }

    public async Task<PropertyGeometryView?> GetGeometryAsync(PropertyId id, CancellationToken cancellationToken = default)
    {
        var document = await store.GetAsync<PropertyDocument>(
            StorageNamespaces.Properties,
            $"property-{id.Value}",
            cancellationToken);

        if (document is null)
        {
            return null;
        }

        var property = document.ToDomain();

        return new PropertyGeometryView(
            property.Id,
            property.Registration.Formatted,
            property.Geometry,
            property.LandArea.SquareMeters);
    }
}