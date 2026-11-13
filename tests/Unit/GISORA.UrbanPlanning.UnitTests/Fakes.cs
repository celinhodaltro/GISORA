using GISORA.Geospatial.Application;
using GISORA.Geospatial.Domain;
using GISORA.UrbanPlanning.Domain;

namespace GISORA.UrbanPlanning.UnitTests;

internal static class PlanningFactory
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

    public static Zone NewZone(string code = "ZR1")
        => Zone.Define(
            code,
            "Zona residencial",
            ZoneParameters.Create(maxOccupancyPercent: 60m),
            Square(),
            Area.FromHectares(5m));
}

internal sealed class InMemoryZoneRepository : IZoneRepository
{
    private readonly List<Zone> _items = [];

    public Task<Zone?> FindAsync(ZoneId id, CancellationToken cancellationToken = default)
        => Task.FromResult(_items.FirstOrDefault(zone => zone.Id == id));

    public Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
        => Task.FromResult(_items.Any(zone => string.Equals(zone.Code, code.Trim(), StringComparison.OrdinalIgnoreCase)));

    public Task AddAsync(Zone zone, CancellationToken cancellationToken = default)
    {
        _items.Add(zone);

        return Task.CompletedTask;
    }

    public Task UpdateAsync(Zone zone, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}

internal sealed class InMemoryRestrictionRepository : IRestrictionRepository
{
    public List<Restriction> Items { get; } = [];

    public Task AddAsync(Restriction restriction, CancellationToken cancellationToken = default)
    {
        Items.Add(restriction);

        return Task.CompletedTask;
    }
}

internal sealed class FakeSpatialOperations : ISpatialOperations
{
    public decimal AreaSquareMeters { get; set; } = 10_000m;

    public Task<Area> AreaAsync(Geometry geometry, CancellationToken cancellationToken = default)
        => Task.FromResult(Area.FromSquareMeters(AreaSquareMeters));

    public Task<Distance> DistanceAsync(Geometry origin, Geometry target, CancellationToken cancellationToken = default)
        => Task.FromResult(Distance.Zero);

    public Task<bool> IntersectsAsync(Geometry left, Geometry right, CancellationToken cancellationToken = default)
        => Task.FromResult(true);

    public Task<bool> ContainsAsync(Geometry container, Geometry contained, CancellationToken cancellationToken = default)
        => Task.FromResult(true);

    public Task<Geometry?> IntersectionAsync(Geometry left, Geometry right, CancellationToken cancellationToken = default)
        => Task.FromResult<Geometry?>(null);
}

internal sealed class FakeGeometryValidator(bool isValid) : IGeometryValidator
{
    public Task<GeometryValidationResult> ValidateAsync(Geometry geometry, CancellationToken cancellationToken = default)
        => Task.FromResult(isValid ? GeometryValidationResult.Valid : GeometryValidationResult.Invalid("anel aberto"));
}