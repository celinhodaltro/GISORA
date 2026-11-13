using GISORA.Domain.Cemetery;
using GISORA.Infrastructure.Persistence.Documents;

namespace GISORA.Infrastructure.Persistence.Repositories;

public sealed class LocalStorageCemeterySectorRepository(ILocalStore store) : ICemeterySectorRepository
{
    private const string IndexKey = "cemetery-sector-index";
    private const string IdsKey = "cemetery-sector-ids";

    public async Task<CemeterySector?> FindAsync(CemeterySectorId id, CancellationToken cancellationToken = default)
    {
        var document = await store.GetAsync<CemeterySectorDocument>(
            StorageNamespaces.Cemetery,
            $"cemetery-sector-{id.Value}",
            cancellationToken);

        return document?.ToDomain();
    }

    public async Task<bool> ExistsByCodeAsync(CemeteryId cemeteryId, string code, CancellationToken cancellationToken = default)
    {
        var index = await ReadIndexAsync(cancellationToken);
        var normalized = code.Trim().ToUpperInvariant();

        return index.Any(entry => entry.CemeteryId == cemeteryId.Value && entry.CodeSearch == normalized);
    }

    public async Task AddAsync(CemeterySector sector, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sector);

        await store.SetAsync(
            StorageNamespaces.Cemetery,
            $"cemetery-sector-{sector.Id.Value}",
            CemeterySectorDocument.FromDomain(sector),
            cancellationToken);

        var ids = await ReadIdsAsync(cancellationToken);
        if (ids.Contains(sector.Id.Value) is false)
        {
            ids.Add(sector.Id.Value);
            await store.SetAsync(StorageNamespaces.Cemetery, IdsKey, ids, cancellationToken);
        }

        await UpsertIndexAsync(sector, cancellationToken);
    }

    public async Task UpdateAsync(CemeterySector sector, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sector);

        if (await store.ExistsAsync(StorageNamespaces.Cemetery, $"cemetery-sector-{sector.Id.Value}", cancellationToken) is false)
        {
            throw new StorageException($"Setor '{sector.Id}' nao encontrado no armazenamento local.");
        }

        await store.SetAsync(
            StorageNamespaces.Cemetery,
            $"cemetery-sector-{sector.Id.Value}",
            CemeterySectorDocument.FromDomain(sector),
            cancellationToken);

        await UpsertIndexAsync(sector, cancellationToken);
    }

    private async Task UpsertIndexAsync(CemeterySector sector, CancellationToken cancellationToken)
    {
        var index = await ReadIndexAsync(cancellationToken);
        index.RemoveAll(entry => entry.Id == sector.Id.Value);
        index.Add(new CemeterySectorIndexEntry
        {
            Id = sector.Id.Value,
            CemeteryId = sector.CemeteryId.Value,
            Code = sector.Code,
            CodeSearch = sector.Code.ToUpperInvariant(),
        });

        await store.SetAsync(StorageNamespaces.Cemetery, IndexKey, index, cancellationToken);
    }

    private async Task<List<Guid>> ReadIdsAsync(CancellationToken cancellationToken)
        => await store.GetAsync<List<Guid>>(StorageNamespaces.Cemetery, IdsKey, cancellationToken) ?? [];

    private async Task<List<CemeterySectorIndexEntry>> ReadIndexAsync(CancellationToken cancellationToken)
        => await store.GetAsync<List<CemeterySectorIndexEntry>>(StorageNamespaces.Cemetery, IndexKey, cancellationToken) ?? [];
}