namespace GISORA.Territory.Domain;

public interface IFiscalSectorRepository
{
    Task<FiscalSector?> FindAsync(FiscalSectorId id, CancellationToken cancellationToken = default);

    Task AddAsync(FiscalSector fiscalSector, CancellationToken cancellationToken = default);

    Task UpdateAsync(FiscalSector fiscalSector, CancellationToken cancellationToken = default);
}