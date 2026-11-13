using GISORA.Application.Rural;
using GISORA.Domain.Rural;
using GISORA.Infrastructure.Persistence.Documents;

namespace GISORA.Infrastructure.Persistence.Repositories;

public sealed class LocalStorageRuralPropertyQueries(ILocalStore store) : IRuralPropertyQueries
{
    private const string IndexKey = "rural-index";
    private const string IdsKey = "rural-ids";

    public async Task<IReadOnlyList<RuralPropertyListItem>> ListAsync(CancellationToken cancellationToken = default)
    {
        var index = await store.GetAsync<List<RuralPropertyIndexEntry>>(StorageNamespaces.Rural, IndexKey, cancellationToken) ?? [];

        return index
            .OrderBy(entry => entry.Name, StringComparer.Ordinal)
            .Select(entry => new RuralPropertyListItem(
                new RuralPropertyId(entry.Id),
                entry.Name,
                entry.Usage,
                entry.CarCode,
                entry.AreaSquareMeters / 10_000m,
                entry.ConstrainedSquareMeters / 10_000m,
                entry.PreservationAreaCount,
                entry.SourceKind))
            .ToArray();
    }

    public async Task<IReadOnlyList<RuralPropertyMapItem>> GetMapItemsAsync(CancellationToken cancellationToken = default)
    {
        var properties = await ReadDocumentsAsync(cancellationToken);

        return properties
            .Select(property => new RuralPropertyMapItem(property.Id, property.Name, property.Boundary))
            .ToArray();
    }

    public async Task<IReadOnlyList<PreservationAreaListItem>> ListPreservationAreasAsync(
        RuralPropertyId? propertyId = null,
        CancellationToken cancellationToken = default)
    {
        var properties = await ReadDocumentsAsync(cancellationToken);

        return properties
            .Where(property => propertyId is null || property.Id == propertyId)
            .SelectMany(property => property.PreservationAreas.Select(area => new PreservationAreaListItem(
                area.Id,
                property.Id,
                area.Label,
                area.Area.Hectares)))
            .ToArray();
    }

    public async Task<IReadOnlyList<PreservationAreaMapItem>> GetPreservationAreaMapItemsAsync(
        CancellationToken cancellationToken = default)
    {
        var properties = await ReadDocumentsAsync(cancellationToken);

        return properties
            .SelectMany(property => property.PreservationAreas.Select(area => new PreservationAreaMapItem(
                area.Id,
                property.Id,
                area.Label,
                area.Boundary)))
            .ToArray();
    }

    private async Task<IReadOnlyList<RuralProperty>> ReadDocumentsAsync(CancellationToken cancellationToken)
    {
        var ids = await store.GetAsync<List<Guid>>(StorageNamespaces.Rural, IdsKey, cancellationToken) ?? [];
        var properties = new List<RuralProperty>(ids.Count);

        foreach (var id in ids)
        {
            var document = await store.GetAsync<RuralPropertyDocument>(
                StorageNamespaces.Rural,
                $"rural-{id}",
                cancellationToken);

            if (document is not null)
            {
                properties.Add(document.ToDomain());
            }
        }

        return properties;
    }
}