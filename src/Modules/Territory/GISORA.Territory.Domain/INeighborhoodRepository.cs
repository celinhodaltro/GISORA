namespace GISORA.Territory.Domain;

public interface INeighborhoodRepository
{
    Task<Neighborhood?> FindAsync(NeighborhoodId id, CancellationToken cancellationToken = default);

    Task AddAsync(Neighborhood neighborhood, CancellationToken cancellationToken = default);

    Task UpdateAsync(Neighborhood neighborhood, CancellationToken cancellationToken = default);
}