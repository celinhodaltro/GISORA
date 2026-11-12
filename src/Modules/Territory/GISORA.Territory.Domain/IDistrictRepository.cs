namespace GISORA.Territory.Domain;

public interface IDistrictRepository
{
    Task<District?> FindAsync(DistrictId id, CancellationToken cancellationToken = default);

    Task AddAsync(District district, CancellationToken cancellationToken = default);

    Task UpdateAsync(District district, CancellationToken cancellationToken = default);
}