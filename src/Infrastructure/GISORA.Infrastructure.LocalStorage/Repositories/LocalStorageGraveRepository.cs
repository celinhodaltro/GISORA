using GISORA.Cemetery.Domain;
using GISORA.Infrastructure.LocalStorage.Persistence;

namespace GISORA.Infrastructure.LocalStorage.Repositories;

public sealed class LocalStorageGraveRepository(ILocalStore store) : IGraveRepository
{
    private const string IndexKey = "grave-index";
    private const string IdsKey = "grave-ids";

    public async Task<Grave?> FindAsync(GraveId id, CancellationToken cancellationToken = default)
    {
        var document = await store.GetAsync<GraveDocument>(StorageNamespaces.Cemetery, $"grave-{id.Value}", cancellationToken);

        return document?.ToDomain();
    }

    public async Task<bool> ExistsByCodeAsync(CemeterySectorId sectorId, string code, CancellationToken cancellationToken = default)
    {
        var index = await ReadIndexAsync(cancellationToken);
        var normalized = code.Trim().ToUpperInvariant();

        return index.Any(entry => entry.SectorId == sectorId.Value && entry.CodeSearch == normalized);
    }

    public async Task AddAsync(Grave grave, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(grave);

        await store.SetAsync(StorageNamespaces.Cemetery, $"grave-{grave.Id.Value}", GraveDocument.FromDomain(grave), cancellationToken);

        var ids = await ReadIdsAsync(cancellationToken);
        if (ids.Contains(grave.Id.Value) is false)
        {
            ids.Add(grave.Id.Value);
            await store.SetAsync(StorageNamespaces.Cemetery, IdsKey, ids, cancellationToken);
        }

        await UpsertIndexAsync(grave, cancellationToken);
    }

    public async Task UpdateAsync(Grave grave, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(grave);

        if (await store.ExistsAsync(StorageNamespaces.Cemetery, $"grave-{grave.Id.Value}", cancellationToken) is false)
        {
            throw new StorageException($"Jazigo '{grave.Id}' nao encontrado no armazenamento local.");
        }

        await store.SetAsync(StorageNamespaces.Cemetery, $"grave-{grave.Id.Value}", GraveDocument.FromDomain(grave), cancellationToken);
        await UpsertIndexAsync(grave, cancellationToken);
    }

    private async Task UpsertIndexAsync(Grave grave, CancellationToken cancellationToken)
    {
        var index = await ReadIndexAsync(cancellationToken);
        index.RemoveAll(entry => entry.Id == grave.Id.Value);
        index.Add(new GraveIndexEntry
        {
            Id = grave.Id.Value,
            CemeteryId = grave.CemeteryId.Value,
            SectorId = grave.SectorId.Value,
            Code = grave.Code,
            CodeSearch = grave.Code.ToUpperInvariant(),
            IsOccupied = grave.IsOccupied,
            ActiveBurialId = grave.ActiveBurial?.Id,
            ActiveBurialPerson = grave.ActiveBurial?.PersonName,
        });

        await store.SetAsync(StorageNamespaces.Cemetery, IndexKey, index, cancellationToken);
    }

    private async Task<List<Guid>> ReadIdsAsync(CancellationToken cancellationToken)
        => await store.GetAsync<List<Guid>>(StorageNamespaces.Cemetery, IdsKey, cancellationToken) ?? [];

    private async Task<List<GraveIndexEntry>> ReadIndexAsync(CancellationToken cancellationToken)
        => await store.GetAsync<List<GraveIndexEntry>>(StorageNamespaces.Cemetery, IndexKey, cancellationToken) ?? [];
}