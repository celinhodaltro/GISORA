using GISORA.Infrastructure.LocalStorage.Persistence;
using GISORA.Territory.Domain;

namespace GISORA.Infrastructure.LocalStorage.Repositories;

public sealed class LocalStorageUrbanBlockRepository(ILocalStore store) : IUrbanBlockRepository
{
    private const string IndexKey = "urbanblock-index";
    private const string IdsKey = "urbanblock-ids";

    public async Task<UrbanBlock?> FindAsync(UrbanBlockId id, CancellationToken cancellationToken = default)
    {
        var document = await store.GetAsync<UrbanBlockDocument>(StorageNamespaces.Territory, Key(id), cancellationToken);

        return document?.ToDomain();
    }

    public async Task<bool> ExistsByCodeAsync(FiscalSectorId fiscalSectorId, string code, CancellationToken cancellationToken = default)
    {
        var normalized = code.Trim().ToUpperInvariant();
        var index = await ReadIndexAsync(cancellationToken);

        return index.Any(entry => entry.FiscalSectorId == fiscalSectorId.Value && entry.CodeSearch == normalized);
    }

    public async Task AddAsync(UrbanBlock urbanBlock, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(urbanBlock);

        var index = await ReadIndexAsync(cancellationToken);
        var normalized = urbanBlock.Code.ToUpperInvariant();

        if (index.Any(entry => entry.FiscalSectorId == urbanBlock.FiscalSectorId.Value && entry.CodeSearch == normalized))
        {
            throw new StorageException("Ja existe quadra com este codigo no setor fiscal.");
        }

        await store.SetAsync(StorageNamespaces.Territory, Key(urbanBlock.Id), UrbanBlockDocument.FromDomain(urbanBlock), cancellationToken);

        var ids = await ReadIdsAsync(cancellationToken);
        if (ids.Contains(urbanBlock.Id.Value) is false)
        {
            ids.Add(urbanBlock.Id.Value);
            await store.SetAsync(StorageNamespaces.Territory, IdsKey, ids, cancellationToken);
        }

        index.Add(UrbanBlockIndexEntry.FromDomain(urbanBlock));
        await store.SetAsync(StorageNamespaces.Territory, IndexKey, index, cancellationToken);
    }

    public async Task UpdateAsync(UrbanBlock urbanBlock, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(urbanBlock);

        if (await store.ExistsAsync(StorageNamespaces.Territory, Key(urbanBlock.Id), cancellationToken) is false)
        {
            throw new StorageException($"Quadra '{urbanBlock.Id}' nao encontrada no armazenamento local.");
        }

        await store.SetAsync(StorageNamespaces.Territory, Key(urbanBlock.Id), UrbanBlockDocument.FromDomain(urbanBlock), cancellationToken);
        await UpsertIndexAsync(urbanBlock, cancellationToken);
    }

    public async Task RebuildIndexAsync(CancellationToken cancellationToken = default)
    {
        var ids = await ReadIdsAsync(cancellationToken);
        var entries = new List<UrbanBlockIndexEntry>();

        foreach (var id in ids)
        {
            var document = await store.GetAsync<UrbanBlockDocument>(StorageNamespaces.Territory, $"urbanblock-{id}", cancellationToken);
            if (document is not null)
            {
                entries.Add(UrbanBlockIndexEntry.FromDomain(document.ToDomain()));
            }
        }

        await store.SetAsync(StorageNamespaces.Territory, IndexKey, entries, cancellationToken);
    }

    private static string Key(UrbanBlockId id) => $"urbanblock-{id.Value}";

    private async Task<List<Guid>> ReadIdsAsync(CancellationToken cancellationToken)
        => await store.GetAsync<List<Guid>>(StorageNamespaces.Territory, IdsKey, cancellationToken) ?? [];

    private async Task<List<UrbanBlockIndexEntry>> ReadIndexAsync(CancellationToken cancellationToken)
        => await store.GetAsync<List<UrbanBlockIndexEntry>>(StorageNamespaces.Territory, IndexKey, cancellationToken) ?? [];

    private async Task UpsertIndexAsync(UrbanBlock urbanBlock, CancellationToken cancellationToken)
    {
        var index = await ReadIndexAsync(cancellationToken);

        index.RemoveAll(entry => entry.Id == urbanBlock.Id.Value);
        index.Add(UrbanBlockIndexEntry.FromDomain(urbanBlock));

        await store.SetAsync(StorageNamespaces.Territory, IndexKey, index, cancellationToken);
    }
}