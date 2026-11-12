using GISORA.Infrastructure.LocalStorage.Persistence;
using GISORA.Territory.Domain;

namespace GISORA.Infrastructure.LocalStorage.Repositories;

public sealed class LocalStorageDistrictRepository(ILocalStore store) : IDistrictRepository
{
    public async Task<District?> FindAsync(DistrictId id, CancellationToken cancellationToken = default)
    {
        var document = await store.GetAsync<DistrictDocument>(StorageNamespaces.Territory, Key(id), cancellationToken);

        return document?.ToDomain();
    }

    public Task AddAsync(District district, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(district);

        return store.SetAsync(StorageNamespaces.Territory, Key(district.Id), DistrictDocument.FromDomain(district), cancellationToken);
    }

    public async Task UpdateAsync(District district, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(district);

        if (await store.ExistsAsync(StorageNamespaces.Territory, Key(district.Id), cancellationToken) is false)
        {
            throw new StorageException($"Distrito '{district.Id}' nao encontrado no armazenamento local.");
        }

        await store.SetAsync(StorageNamespaces.Territory, Key(district.Id), DistrictDocument.FromDomain(district), cancellationToken);
    }

    private static string Key(DistrictId id) => $"district-{id.Value}";
}