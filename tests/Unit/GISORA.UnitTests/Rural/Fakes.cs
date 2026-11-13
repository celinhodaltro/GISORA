using GISORA.Application.Geospatial;
using GISORA.Domain.Geospatial;
using GISORA.Domain.Rural;

namespace GISORA.UnitTests.Rural;

internal static class RuralFactory
{
    public static Polygon Square(double offset = 0)
    {
        var reference = SpatialReference.Wgs84;

        return new Polygon(
        [
            new CoordinateSequence(
            [
                new Coordinate(-35.26 + offset, -7.96 + offset, reference),
                new Coordinate(-35.24 + offset, -7.96 + offset, reference),
                new Coordinate(-35.24 + offset, -7.94 + offset, reference),
                new Coordinate(-35.26 + offset, -7.94 + offset, reference),
                new Coordinate(-35.26 + offset, -7.96 + offset, reference),
            ]),
        ]);
    }

    public static RuralProperty New(string name = "Fazenda Boa Vista", decimal? constrained = null)
    {
        var property = RuralProperty.Register(
            name,
            Square(),
            Area.FromHectares(100m),
            RuralUsage.Agriculture,
            carCode: "PE-2610509-ABCD.EFGH.IJKL.MNOP.QRST");

        if (constrained is { } area)
        {
            property.DefineLegalReserve(Square(), Area.FromHectares(area));
        }

        return property;
    }
}

internal sealed class InMemoryRuralRepository : IRuralPropertyRepository
{
    private readonly Dictionary<RuralPropertyId, RuralProperty> _items = [];

    public IReadOnlyCollection<RuralProperty> Items => _items.Values;

    public Task<RuralProperty?> FindAsync(RuralPropertyId id, CancellationToken cancellationToken = default)
        => Task.FromResult(_items.GetValueOrDefault(id));

    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
        => Task.FromResult(_items.Values.Any(item => string.Equals(item.Name, name.Trim(), StringComparison.OrdinalIgnoreCase)));

    public Task AddAsync(RuralProperty property, CancellationToken cancellationToken = default)
    {
        _items[property.Id] = property;

        return Task.CompletedTask;
    }

    public Task UpdateAsync(RuralProperty property, CancellationToken cancellationToken = default)
    {
        _items[property.Id] = property;

        return Task.CompletedTask;
    }
}

internal sealed class FakeSpatialOperations : ISpatialOperations
{
    private readonly Queue<decimal> _areas = new();

    public decimal AreaSquareMeters { get; set; } = 1_000_000m;

    public bool IntersectsResult { get; set; }

    public Geometry? IntersectionResult { get; set; }

    public FakeSpatialOperations WithArea(decimal squareMeters)
    {
        _areas.Enqueue(squareMeters);

        return this;
    }

    public Task<Area> AreaAsync(Geometry geometry, CancellationToken cancellationToken = default)
        => Task.FromResult(Area.FromSquareMeters(_areas.Count > 0 ? _areas.Dequeue() : AreaSquareMeters));

    public Task<Distance> DistanceAsync(Geometry origin, Geometry target, CancellationToken cancellationToken = default)
        => Task.FromResult(Distance.Zero);

    public Task<bool> IntersectsAsync(Geometry left, Geometry right, CancellationToken cancellationToken = default)
        => Task.FromResult(IntersectsResult);

    public Task<bool> ContainsAsync(Geometry container, Geometry contained, CancellationToken cancellationToken = default)
        => Task.FromResult(true);

    public Task<Geometry?> IntersectionAsync(Geometry left, Geometry right, CancellationToken cancellationToken = default)
        => Task.FromResult(IntersectionResult);
}

internal sealed class FakeGeoJsonSerializer(Geometry? geometry) : IGeoJsonSerializer
{
    public string Serialize(Geometry geometry) => "{}";

    public Geometry Deserialize(string geoJson, SpatialReference reference) => geometry!;
}

internal sealed class FakeGeometryValidator(bool isValid = true) : IGeometryValidator
{
    public Task<GeometryValidationResult> ValidateAsync(Geometry geometry, CancellationToken cancellationToken = default)
        => Task.FromResult(isValid ? GeometryValidationResult.Valid : GeometryValidationResult.Invalid("anel aberto"));
}