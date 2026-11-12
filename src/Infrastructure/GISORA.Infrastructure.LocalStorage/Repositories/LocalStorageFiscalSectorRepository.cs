using GISORA.Infrastructure.LocalStorage.Persistence;
using GISORA.Territory.Domain;

namespace GISORA.Infrastructure.LocalStorage.Repositories;

public sealed class LocalStorageFiscalSectorRepository(ILocalStore store) : IFiscalSectorRepository
{
    public async Task<FiscalSector?> FindAsync(FiscalSectorId id, CancellationToken cancellationToken = default)
    {
        var document = await store.GetAsync<FiscalSectorDocument>(StorageNamespaces.Territory, Key(id), cancellationToken);

        return document?.ToDomain();
    }

    public Task AddAsync(FiscalSector fiscalSector, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fiscalSector);

        return store.SetAsync(StorageNamespaces.Territory, Key(fiscalSector.Id), FiscalSectorDocument.FromDomain(fiscalSector), cancellationToken);
    }

    public async Task UpdateAsync(FiscalSector fiscalSector, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fiscalSector);

        if (await store.ExistsAsync(StorageNamespaces.Territory, Key(fiscalSector.Id), cancellationToken) is false)
        {
            throw new StorageException($"Setor fiscal '{fiscalSector.Id}' nao encontrado no armazenamento local.");
        }

        await store.SetAsync(StorageNamespaces.Territory, Key(fiscalSector.Id), FiscalSectorDocument.FromDomain(fiscalSector), cancellationToken);
    }

    private static string Key(FiscalSectorId id) => $"fiscal-sector-{id.Value}";
}