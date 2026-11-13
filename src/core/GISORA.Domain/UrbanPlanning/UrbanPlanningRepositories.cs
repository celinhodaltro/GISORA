namespace GISORA.Domain.UrbanPlanning;

public interface IZoneRepository
{
    Task<Zone?> FindAsync(ZoneId id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task AddAsync(Zone zone, CancellationToken cancellationToken = default);

    Task UpdateAsync(Zone zone, CancellationToken cancellationToken = default);
}

public interface IRestrictionRepository
{
    Task AddAsync(Restriction restriction, CancellationToken cancellationToken = default);
}