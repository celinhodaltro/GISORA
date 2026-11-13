namespace GISORA.Rural.Domain;

public interface IRuralPropertyRepository
{
    Task<RuralProperty?> FindAsync(RuralPropertyId id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);

    Task AddAsync(RuralProperty property, CancellationToken cancellationToken = default);

    Task UpdateAsync(RuralProperty property, CancellationToken cancellationToken = default);
}