namespace GISORA.Domain.Territory;

public interface IMunicipalityRepository
{
    Task<Municipality?> FindAsync(MunicipalityId id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByIbgeCodeAsync(IbgeCode code, CancellationToken cancellationToken = default);

    Task AddAsync(Municipality municipality, CancellationToken cancellationToken = default);

    Task UpdateAsync(Municipality municipality, CancellationToken cancellationToken = default);
}