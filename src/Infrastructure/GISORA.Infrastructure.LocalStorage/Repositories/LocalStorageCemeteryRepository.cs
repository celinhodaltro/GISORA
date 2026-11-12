using GISORA.Cemetery.Domain;
using GISORA.Infrastructure.LocalStorage.Persistence;

using CemeteryAggregate = GISORA.Cemetery.Domain.Cemetery;

namespace GISORA.Infrastructure.LocalStorage.Repositories;

public sealed class LocalStorageCemeteryRepository(ILocalStore store) : ICemeteryRepository
{
    private const string IndexKey = "cemetery-index";
    private const string IdsKey = "cemetery-ids";

    public async Task<CemeteryAggregate?> FindAsync(CemeteryId id, CancellationToken cancellationToken = default)
    {
        var document = await store.GetAsync<CemeteryDocument>(StorageNamespaces.Cemetery, $"cemetery-{id.Value}", cancellationToken);

        return document?.ToDomain();
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var index = await ReadIndexAsync(cancellationToken);
        var trimmed = name.Trim();

        return index.Any(entry => string.Equals(entry.Name, trimmed, StringComparison.OrdinalIgnoreCase));
    }

    public async Task AddAsync(CemeteryAggregate cemetery, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cemetery);

        await store.SetAsync(
            StorageNamespaces.Cemetery,
            $"cemetery-{cemetery.Id.Value}",
            CemeteryDocument.FromDomain(cemetery),
            cancellationToken);

        var ids = await ReadIdsAsync(cancellationToken);
        if (ids.Contains(cemetery.Id.Value) is false)
        {
            ids.Add(cemetery.Id.Value);
            await store.SetAsync(StorageNamespaces.Cemetery, IdsKey, ids, cancellationToken);
        }

        var index = await ReadIndexAsync(cancellationToken);
        index.RemoveAll(entry => entry.Id == cemetery.Id.Value);
        index.Add(new CemeteryIndexEntry
        {
            Id = cemetery.Id.Value,
            Name = cemetery.Name,
            AreaSquareMeters = cemetery.Area.SquareMeters,
        });

        await store.SetAsync(StorageNamespaces.Cemetery, IndexKey, index, cancellationToken);
    }

    public async Task UpdateAsync(CemeteryAggregate cemetery, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cemetery);

        if (await store.ExistsAsync(StorageNamespaces.Cemetery, $"cemetery-{cemetery.Id.Value}", cancellationToken) is false)
        {
            throw new StorageException($"Cemiterio '{cemetery.Id}' nao encontrado no armazenamento local.");
        }

        await store.SetAsync(
            StorageNamespaces.Cemetery,
            $"cemetery-{cemetery.Id.Value}",
            CemeteryDocument.FromDomain(cemetery),
            cancellationToken);

        var index = await ReadIndexAsync(cancellationToken);
        index.RemoveAll(entry => entry.Id == cemetery.Id.Value);
        index.Add(new CemeteryIndexEntry
        {
            Id = cemetery.Id.Value,
            Name = cemetery.Name,
            AreaSquareMeters = cemetery.Area.SquareMeters,
        });

        await store.SetAsync(StorageNamespaces.Cemetery, IndexKey, index, cancellationToken);
    }

    private async Task<List<Guid>> ReadIdsAsync(CancellationToken cancellationToken)
        => await store.GetAsync<List<Guid>>(StorageNamespaces.Cemetery, IdsKey, cancellationToken) ?? [];

    private async Task<List<CemeteryIndexEntry>> ReadIndexAsync(CancellationToken cancellationToken)
        => await store.GetAsync<List<CemeteryIndexEntry>>(StorageNamespaces.Cemetery, IndexKey, cancellationToken) ?? [];
}