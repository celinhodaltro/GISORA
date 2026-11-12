using GISORA.Properties.Application;
using GISORA.Properties.Domain;

namespace GISORA.Properties.UnitTests.Fakes;

internal sealed class InMemoryPropertyRepository : IPropertyRepository
{
    private readonly Dictionary<PropertyId, Property> _items = [];

    public Task<Property?> FindAsync(PropertyId id, CancellationToken cancellationToken = default)
        => Task.FromResult(_items.GetValueOrDefault(id));

    public Task<bool> ExistsByRegistrationAsync(PropertyRegistration registration, CancellationToken cancellationToken = default)
        => Task.FromResult(_items.Values.Any(property => property.Registration == registration));

    public Task AddAsync(Property property, CancellationToken cancellationToken = default)
    {
        _items.Add(property.Id, property);

        return Task.CompletedTask;
    }

    public Task UpdateAsync(Property property, CancellationToken cancellationToken = default)
    {
        _items[property.Id] = property;

        return Task.CompletedTask;
    }
}

internal sealed class InMemoryTaxpayerRepository : ITaxpayerRepository
{
    private readonly Dictionary<TaxpayerId, Taxpayer> _items = [];

    public Task<Taxpayer?> FindAsync(TaxpayerId id, CancellationToken cancellationToken = default)
        => Task.FromResult(_items.GetValueOrDefault(id));

    public Task<bool> ExistsByDocumentAsync(DocumentNumber document, CancellationToken cancellationToken = default)
        => Task.FromResult(_items.Values.Any(taxpayer => taxpayer.Document == document));

    public Task AddAsync(Taxpayer taxpayer, CancellationToken cancellationToken = default)
    {
        _items.Add(taxpayer.Id, taxpayer);

        return Task.CompletedTask;
    }

    public Task UpdateAsync(Taxpayer taxpayer, CancellationToken cancellationToken = default)
    {
        _items[taxpayer.Id] = taxpayer;

        return Task.CompletedTask;
    }
}

internal sealed class FakePropertyQueries : IPropertyQueries
{
    public PropertyFilter? LastFilter { get; private set; }

    public Task<GISORA.BuildingBlocks.Application.PagedResult<PropertyListItem>> SearchAsync(
        PropertyFilter filter,
        CancellationToken cancellationToken = default)
    {
        LastFilter = filter;

        return Task.FromResult(new GISORA.BuildingBlocks.Application.PagedResult<PropertyListItem>(
            [],
            filter.PageNumber,
            filter.PageSize,
            0));
    }

    public Task<PropertyGeometryView?> GetGeometryAsync(PropertyId id, CancellationToken cancellationToken = default)
        => Task.FromResult<PropertyGeometryView?>(null);
}