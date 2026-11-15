using GISORA.Application.Geospatial;
using GISORA.Application.Properties;
using GISORA.Domain.Geospatial;
using GISORA.Domain.Properties;

namespace GISORA.UnitTests.Properties.Fakes;

internal sealed class InMemoryPropertyRepository : IPropertyRepository
{
    private readonly Dictionary<PropertyId, Property> _items = [];

    public IReadOnlyCollection<Property> Items => _items.Values;

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

    public Task<GISORA.Application.Common.PagedResult<PropertyListItem>> SearchAsync(
        PropertyFilter filter,
        CancellationToken cancellationToken = default)
    {
        LastFilter = filter;

        return Task.FromResult(new GISORA.Application.Common.PagedResult<PropertyListItem>(
            [],
            filter.PageNumber,
            filter.PageSize,
            0));
    }

    public Task<PropertyGeometryView?> GetGeometryAsync(PropertyId id, CancellationToken cancellationToken = default)
        => Task.FromResult<PropertyGeometryView?>(null);
}

internal sealed class FakeGeometryValidator : IGeometryValidator
{
    public GeometryValidationResult Result { get; set; } = GeometryValidationResult.Valid;

    public Task<GeometryValidationResult> ValidateAsync(Geometry geometry, CancellationToken cancellationToken = default)
        => Task.FromResult(Result);
}

internal sealed class FakePropertyGeometryValidator : IPropertyGeometryValidator
{
    public GeometryValidationResult Result { get; set; } = GeometryValidationResult.Valid;

    public GeometryValidationResult? LastResult { get; private set; }

    public PropertyId? LastExcludedPropertyId { get; private set; }

    public Task<GeometryValidationResult> ValidateAsync(
        Geometry geometry,
        PropertyId? excludedPropertyId = null,
        CancellationToken cancellationToken = default)
    {
        LastExcludedPropertyId = excludedPropertyId;
        LastResult = Result;

        return Task.FromResult(Result);
    }
}

internal sealed class InMemoryPropertyMapQueries : IPropertyMapQueries
{
    public List<PropertyMapItem> Items { get; } = [];

    public Task<IReadOnlyList<PropertyMapItem>> GetMapItemsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<PropertyMapItem>>(Items);
}

internal sealed class FakeSpatialOperations : ISpatialOperations
{
    public Func<Geometry, Geometry, bool> Intersects { get; set; } = (_, _) => false;

    public Func<Geometry, Geometry, Geometry?> Intersection { get; set; } = (_, _) => null;

    public decimal AreaSquareMeters { get; set; } = 100m;

    public Task<Area> AreaAsync(Geometry geometry, CancellationToken cancellationToken = default)
        => Task.FromResult(Area.FromSquareMeters(AreaSquareMeters));

    public Task<Distance> DistanceAsync(Geometry origin, Geometry target, CancellationToken cancellationToken = default)
        => Task.FromResult(Distance.FromMeters(0m));

    public Task<bool> IntersectsAsync(Geometry left, Geometry right, CancellationToken cancellationToken = default)
        => Task.FromResult(Intersects(left, right));

    public Task<bool> ContainsAsync(Geometry container, Geometry contained, CancellationToken cancellationToken = default)
        => Task.FromResult(false);

    public Task<Geometry?> IntersectionAsync(Geometry left, Geometry right, CancellationToken cancellationToken = default)
        => Task.FromResult(Intersection(left, right));
}