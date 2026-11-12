namespace GISORA.Properties.Domain;

public interface IPropertyRepository
{
    Task<Property?> FindAsync(PropertyId id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByRegistrationAsync(PropertyRegistration registration, CancellationToken cancellationToken = default);

    Task AddAsync(Property property, CancellationToken cancellationToken = default);

    Task UpdateAsync(Property property, CancellationToken cancellationToken = default);
}