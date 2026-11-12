using GISORA.Infrastructure.LocalStorage.Persistence;
using GISORA.Territory.Domain;

namespace GISORA.Infrastructure.LocalStorage.Repositories;

public sealed class LocalStorageNeighborhoodRepository(ILocalStore store) : INeighborhoodRepository
{
    public async Task<Neighborhood?> FindAsync(NeighborhoodId id, CancellationToken cancellationToken = default)
    {
        var document = await store.GetAsync<NeighborhoodDocument>(StorageNamespaces.Territory, Key(id), cancellationToken);

        return document?.ToDomain();
    }

    public Task AddAsync(Neighborhood neighborhood, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(neighborhood);

        return store.SetAsync(StorageNamespaces.Territory, Key(neighborhood.Id), NeighborhoodDocument.FromDomain(neighborhood), cancellationToken);
    }

    public async Task UpdateAsync(Neighborhood neighborhood, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(neighborhood);

        if (await store.ExistsAsync(StorageNamespaces.Territory, Key(neighborhood.Id), cancellationToken) is false)
        {
            throw new StorageException($"Bairro '{neighborhood.Id}' nao encontrado no armazenamento local.");
        }

        await store.SetAsync(StorageNamespaces.Territory, Key(neighborhood.Id), NeighborhoodDocument.FromDomain(neighborhood), cancellationToken);
    }

    private static string Key(NeighborhoodId id) => $"neighborhood-{id.Value}";
}