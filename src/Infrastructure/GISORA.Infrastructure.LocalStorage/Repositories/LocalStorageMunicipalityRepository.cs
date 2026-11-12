using GISORA.Infrastructure.LocalStorage.Persistence;
using GISORA.Territory.Domain;

namespace GISORA.Infrastructure.LocalStorage.Repositories;

public sealed class LocalStorageMunicipalityRepository(ILocalStore store) : IMunicipalityRepository
{
    private const string IbgeIndexKey = "municipality-ibge-index";

    private const string IdsKey = "municipality-ids";

    public async Task<Municipality?> FindAsync(MunicipalityId id, CancellationToken cancellationToken = default)
    {
        var document = await store.GetAsync<MunicipalityDocument>(StorageNamespaces.Territory, Key(id), cancellationToken);

        return document?.ToDomain();
    }

    public async Task<bool> ExistsByIbgeCodeAsync(IbgeCode code, CancellationToken cancellationToken = default)
    {
        var index = await ReadIbgeIndexAsync(cancellationToken);

        return index.ContainsKey(code.Value);
    }

    public async Task AddAsync(Municipality municipality, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(municipality);

        var index = await ReadIbgeIndexAsync(cancellationToken);
        if (index.ContainsKey(municipality.IbgeCode.Value))
        {
            throw new StorageException("Ja existe municipio com este codigo IBGE no armazenamento local.");
        }

        await store.SetAsync(StorageNamespaces.Territory, Key(municipality.Id), MunicipalityDocument.FromDomain(municipality), cancellationToken);

        index[municipality.IbgeCode.Value] = municipality.Id.Value;
        await store.SetAsync(StorageNamespaces.Territory, IbgeIndexKey, index, cancellationToken);

        var ids = await store.GetAsync<List<Guid>>(StorageNamespaces.Territory, IdsKey, cancellationToken) ?? [];
        if (ids.Contains(municipality.Id.Value) is false)
        {
            ids.Add(municipality.Id.Value);
            await store.SetAsync(StorageNamespaces.Territory, IdsKey, ids, cancellationToken);
        }
    }

    public async Task UpdateAsync(Municipality municipality, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(municipality);

        if (await store.ExistsAsync(StorageNamespaces.Territory, Key(municipality.Id), cancellationToken) is false)
        {
            throw new StorageException($"Municipio '{municipality.Id}' nao encontrado no armazenamento local.");
        }

        await store.SetAsync(StorageNamespaces.Territory, Key(municipality.Id), MunicipalityDocument.FromDomain(municipality), cancellationToken);

        var index = await ReadIbgeIndexAsync(cancellationToken);
        foreach (var entry in index.Where(entry => entry.Value == municipality.Id.Value).ToArray())
        {
            index.Remove(entry.Key);
        }

        index[municipality.IbgeCode.Value] = municipality.Id.Value;
        await store.SetAsync(StorageNamespaces.Territory, IbgeIndexKey, index, cancellationToken);
    }

    private static string Key(MunicipalityId id) => $"municipality-{id.Value}";

    private async Task<Dictionary<string, Guid>> ReadIbgeIndexAsync(CancellationToken cancellationToken)
        => await store.GetAsync<Dictionary<string, Guid>>(StorageNamespaces.Territory, IbgeIndexKey, cancellationToken) ?? [];
}