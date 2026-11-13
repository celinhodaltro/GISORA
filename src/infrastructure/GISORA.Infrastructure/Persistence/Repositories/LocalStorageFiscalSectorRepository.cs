using GISORA.Domain.Territory;
using GISORA.Infrastructure.Persistence.Documents;

namespace GISORA.Infrastructure.Persistence.Repositories;

public sealed class LocalStorageFiscalSectorRepository(ILocalStore store) : IFiscalSectorRepository
{
    private const string IndexKey = "fiscal-sector-index";
    private const string IdsKey = "fiscal-sector-ids";

    public async Task<FiscalSector?> FindAsync(FiscalSectorId id, CancellationToken cancellationToken = default)
    {
        var document = await store.GetAsync<FiscalSectorDocument>(StorageNamespaces.Territory, Key(id), cancellationToken);

        return document?.ToDomain();
    }

    public async Task<bool> ExistsByCodeAsync(DistrictId districtId, string code, CancellationToken cancellationToken = default)
    {
        var normalized = code.Trim().ToUpperInvariant();
        var index = await ReadIndexAsync(cancellationToken);

        return index.Any(entry => entry.DistrictId == districtId.Value && entry.CodeSearch == normalized);
    }

    public async Task AddAsync(FiscalSector fiscalSector, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fiscalSector);

        var index = await ReadIndexAsync(cancellationToken);
        var normalized = fiscalSector.Code.ToUpperInvariant();

        if (index.Any(entry => entry.DistrictId == fiscalSector.DistrictId.Value && entry.CodeSearch == normalized))
        {
            throw new StorageException("Ja existe setor fiscal com este codigo no armazenamento local.");
        }

        await store.SetAsync(StorageNamespaces.Territory, Key(fiscalSector.Id), FiscalSectorDocument.FromDomain(fiscalSector), cancellationToken);

        var ids = await ReadIdsAsync(cancellationToken);
        if (ids.Contains(fiscalSector.Id.Value) is false)
        {
            ids.Add(fiscalSector.Id.Value);
            await store.SetAsync(StorageNamespaces.Territory, IdsKey, ids, cancellationToken);
        }

        index.Add(new FiscalSectorIndexEntry
        {
            Id = fiscalSector.Id.Value,
            DistrictId = fiscalSector.DistrictId.Value,
            Code = fiscalSector.Code,
            CodeSearch = normalized,
        });
        await store.SetAsync(StorageNamespaces.Territory, IndexKey, index, cancellationToken);
    }

    public async Task UpdateAsync(FiscalSector fiscalSector, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fiscalSector);

        if (await store.ExistsAsync(StorageNamespaces.Territory, Key(fiscalSector.Id), cancellationToken) is false)
        {
            throw new StorageException($"Setor fiscal '{fiscalSector.Id}' nao encontrado no armazenamento local.");
        }

        await store.SetAsync(StorageNamespaces.Territory, Key(fiscalSector.Id), FiscalSectorDocument.FromDomain(fiscalSector), cancellationToken);

        var index = await ReadIndexAsync(cancellationToken);
        index.RemoveAll(entry => entry.Id == fiscalSector.Id.Value);
        index.Add(new FiscalSectorIndexEntry
        {
            Id = fiscalSector.Id.Value,
            DistrictId = fiscalSector.DistrictId.Value,
            Code = fiscalSector.Code,
            CodeSearch = fiscalSector.Code.ToUpperInvariant(),
        });
        await store.SetAsync(StorageNamespaces.Territory, IndexKey, index, cancellationToken);
    }

    private static string Key(FiscalSectorId id) => $"fiscal-sector-{id.Value}";

    private async Task<List<Guid>> ReadIdsAsync(CancellationToken cancellationToken)
        => await store.GetAsync<List<Guid>>(StorageNamespaces.Territory, IdsKey, cancellationToken) ?? [];

    private async Task<List<FiscalSectorIndexEntry>> ReadIndexAsync(CancellationToken cancellationToken)
        => await store.GetAsync<List<FiscalSectorIndexEntry>>(StorageNamespaces.Territory, IndexKey, cancellationToken) ?? [];
}