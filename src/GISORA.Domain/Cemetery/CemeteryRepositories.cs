namespace GISORA.Domain.Cemetery;

public interface ICemeteryRepository
{
    Task<Cemetery?> FindAsync(CemeteryId id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);

    Task AddAsync(Cemetery cemetery, CancellationToken cancellationToken = default);

    Task UpdateAsync(Cemetery cemetery, CancellationToken cancellationToken = default);
}

public interface ICemeterySectorRepository
{
    Task<CemeterySector?> FindAsync(CemeterySectorId id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByCodeAsync(CemeteryId cemeteryId, string code, CancellationToken cancellationToken = default);

    Task AddAsync(CemeterySector sector, CancellationToken cancellationToken = default);

    Task UpdateAsync(CemeterySector sector, CancellationToken cancellationToken = default);
}

public interface IGraveRepository
{
    Task<Grave?> FindAsync(GraveId id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByCodeAsync(CemeterySectorId sectorId, string code, CancellationToken cancellationToken = default);

    Task AddAsync(Grave grave, CancellationToken cancellationToken = default);

    Task UpdateAsync(Grave grave, CancellationToken cancellationToken = default);
}