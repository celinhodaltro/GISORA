namespace GISORA.Domain.Roads;

public interface IStreetRepository
{
    Task<Street?> FindAsync(StreetId id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(string name, Guid? neighborhoodId, CancellationToken cancellationToken = default);

    Task AddAsync(Street street, CancellationToken cancellationToken = default);

    Task UpdateAsync(Street street, CancellationToken cancellationToken = default);
}