namespace GISORA.Domain.PublicFacilities;

public interface IPublicFacilityRepository
{
    Task<PublicFacility?> FindAsync(PublicFacilityId id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(string name, Guid? neighborhoodId, CancellationToken cancellationToken = default);

    Task AddAsync(PublicFacility facility, CancellationToken cancellationToken = default);

    Task UpdateAsync(PublicFacility facility, CancellationToken cancellationToken = default);
}