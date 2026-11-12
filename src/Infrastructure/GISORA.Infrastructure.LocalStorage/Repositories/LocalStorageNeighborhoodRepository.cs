using GISORA.Infrastructure.LocalStorage.Persistence;
using GISORA.Territory.Domain;

namespace GISORA.Infrastructure.LocalStorage.Repositories;

public sealed class LocalStorageNeighborhoodRepository(ILocalStore store) : INeighborhoodRepository
{
    private const string IndexKey = "neighborhood-index";
    private const string IdsKey = "neighborhood-ids";

    public async Task<Neighborhood?> FindAsync(NeighborhoodId id, CancellationToken cancellationToken = default)
    {
        var document = await store.GetAsync<NeighborhoodDocument>(StorageNamespaces.Territory, Key(id), cancellationToken);

        return document?.ToDomain();
    }

    public async Task<bool> ExistsByNameAsync(DistrictId districtId, string name, CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim().ToUpperInvariant();
        var index = await ReadIndexAsync(cancellationToken);

        return index.Any(entry => entry.DistrictId == districtId.Value && entry.NameSearch == normalized);
    }

    public async Task AddAsync(Neighborhood neighborhood, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(neighborhood);

        var index = await ReadIndexAsync(cancellationToken);
        var normalized = neighborhood.Name.ToUpperInvariant();

        if (index.Any(entry => entry.DistrictId == neighborhood.DistrictId.Value && entry.NameSearch == normalized))
        {
            throw new StorageException("Ja existe bairro com este nome no armazenamento local.");
        }

        await store.SetAsync(StorageNamespaces.Territory, Key(neighborhood.Id), NeighborhoodDocument.FromDomain(neighborhood), cancellationToken);

        var ids = await ReadIdsAsync(cancellationToken);
        if (ids.Contains(neighborhood.Id.Value) is false)
        {
            ids.Add(neighborhood.Id.Value);
            await store.SetAsync(StorageNamespaces.Territory, IdsKey, ids, cancellationToken);
        }

        index.Add(new NeighborhoodIndexEntry
        {
            Id = neighborhood.Id.Value,
            DistrictId = neighborhood.DistrictId.Value,
            Name = neighborhood.Name,
            NameSearch = normalized,
        });
        await store.SetAsync(StorageNamespaces.Territory, IndexKey, index, cancellationToken);
    }

    public async Task UpdateAsync(Neighborhood neighborhood, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(neighborhood);

        if (await store.ExistsAsync(StorageNamespaces.Territory, Key(neighborhood.Id), cancellationToken) is false)
        {
            throw new StorageException($"Bairro '{neighborhood.Id}' nao encontrado no armazenamento local.");
        }

        await store.SetAsync(StorageNamespaces.Territory, Key(neighborhood.Id), NeighborhoodDocument.FromDomain(neighborhood), cancellationToken);

        var index = await ReadIndexAsync(cancellationToken);
        index.RemoveAll(entry => entry.Id == neighborhood.Id.Value);
        index.Add(new NeighborhoodIndexEntry
        {
            Id = neighborhood.Id.Value,
            DistrictId = neighborhood.DistrictId.Value,
            Name = neighborhood.Name,
            NameSearch = neighborhood.Name.ToUpperInvariant(),
        });
        await store.SetAsync(StorageNamespaces.Territory, IndexKey, index, cancellationToken);
    }

    private static string Key(NeighborhoodId id) => $"neighborhood-{id.Value}";

    private async Task<List<Guid>> ReadIdsAsync(CancellationToken cancellationToken)
        => await store.GetAsync<List<Guid>>(StorageNamespaces.Territory, IdsKey, cancellationToken) ?? [];

    private async Task<List<NeighborhoodIndexEntry>> ReadIndexAsync(CancellationToken cancellationToken)
        => await store.GetAsync<List<NeighborhoodIndexEntry>>(StorageNamespaces.Territory, IndexKey, cancellationToken) ?? [];
}