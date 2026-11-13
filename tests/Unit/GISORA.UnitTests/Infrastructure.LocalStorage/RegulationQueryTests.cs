using GISORA.Application.Geospatial;
using GISORA.Domain.Geospatial;
using GISORA.Domain.UrbanPlanning;
using GISORA.Infrastructure.Persistence.Repositories;
using GISORA.UnitTests.Infrastructure.LocalStorage.Fakes;

namespace GISORA.UnitTests.Infrastructure.LocalStorage;

public class RegulationQueryTests
{
    [Fact]
    public async Task IncidentLookupDelegatesContainmentToSpatialPort()
    {
        var store = new InMemoryLocalStore();
        var spatial = new RecordingSpatialOperations();

        await new LocalStorageZoneRepository(store).AddAsync(Zone.Define(
            "ZR1",
            "Zona residencial",
            ZoneParameters.Create(maxOccupancyPercent: 60m),
            Square(),
            Area.FromHectares(5m)));

        await new LocalStorageRestrictionRepository(store).AddAsync(Restriction.Define(
            RestrictionType.RiskArea,
            "Encosta instavel",
            Square(),
            Area.FromHectares(1m)));

        var queries = new LocalStorageRegulationQueries(store, spatial);
        var incident = await queries.FindIncidentAsync(Square());

        Assert.Single(incident.Zones);
        Assert.Single(incident.Restrictions);
        Assert.Equal("ZR1", incident.Zones[0].Code);
        Assert.Equal(1, spatial.ContainsCalls);
        Assert.Equal(1, spatial.IntersectsCalls);
    }

    private static Polygon Square()
        => new([
            new CoordinateSequence([
                new Coordinate(-35.26, -7.96, SpatialReference.Wgs84),
                new Coordinate(-35.24, -7.96, SpatialReference.Wgs84),
                new Coordinate(-35.24, -7.94, SpatialReference.Wgs84),
                new Coordinate(-35.26, -7.94, SpatialReference.Wgs84),
                new Coordinate(-35.26, -7.96, SpatialReference.Wgs84),
            ]),
        ]);
}

internal sealed class RecordingSpatialOperations : ISpatialOperations
{
    public int ContainsCalls { get; private set; }

    public int IntersectsCalls { get; private set; }

    public Task<Area> AreaAsync(Geometry geometry, CancellationToken cancellationToken = default)
        => Task.FromResult(Area.FromHectares(1m));

    public Task<Distance> DistanceAsync(Geometry origin, Geometry target, CancellationToken cancellationToken = default)
        => Task.FromResult(Distance.Zero);

    public Task<bool> IntersectsAsync(Geometry left, Geometry right, CancellationToken cancellationToken = default)
    {
        IntersectsCalls++;

        return Task.FromResult(true);
    }

    public Task<bool> ContainsAsync(Geometry container, Geometry contained, CancellationToken cancellationToken = default)
    {
        ContainsCalls++;

        return Task.FromResult(true);
    }

    public Task<Geometry?> IntersectionAsync(Geometry left, Geometry right, CancellationToken cancellationToken = default)
        => Task.FromResult<Geometry?>(null);
}