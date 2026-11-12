namespace GISORA.Territory.Domain;

public interface IUrbanBlockRepository
{
    Task<UrbanBlock?> FindAsync(UrbanBlockId id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByCodeAsync(FiscalSectorId fiscalSectorId, string code, CancellationToken cancellationToken = default);

    Task AddAsync(UrbanBlock urbanBlock, CancellationToken cancellationToken = default);

    Task UpdateAsync(UrbanBlock urbanBlock, CancellationToken cancellationToken = default);
}