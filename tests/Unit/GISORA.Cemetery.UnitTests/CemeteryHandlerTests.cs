using GISORA.Cemetery.Application.Commands;
using GISORA.Cemetery.UnitTests.Fakes;
using GISORA.Geospatial.Domain;

namespace GISORA.Cemetery.UnitTests;

public class CemeteryHandlerTests
{
    private static Point Inside() => new(new Coordinate(-35.2498, -7.9498, SpatialReference.Wgs84));

    [Fact]
    public async Task CreateCemeteryComputesAreaFromSpatialPort()
    {
        var cemeteries = new InMemoryCemeteryRepository();
        var spatial = new FakeSpatialOperations { AreaSquareMeters = 12_345m };
        var handler = new CreateCemeteryHandler(cemeteries, spatial);

        var result = await handler.HandleAsync(new CreateCemeteryCommand(
            "Cemiterio Municipal",
            CemeteryFactory.Square(-35.25, -7.95),
            null));

        Assert.True(result.IsSuccess);

        var cemetery = cemeteries.Items[result.Value];
        Assert.Equal(12_345m, cemetery.Area.SquareMeters);
    }

    [Fact]
    public async Task GraveOutsideCemeteryIsRejected()
    {
        var cemeteries = new InMemoryCemeteryRepository();
        var sectors = new InMemoryCemeterySectorRepository();
        var graves = new InMemoryGraveRepository();
        var spatial = new FakeSpatialOperations { ContainsResult = false };

        var cemetery = CemeteryFactory.New();
        await cemeteries.AddAsync(cemetery);
        var sector = CemeteryFactory.NewSector(cemetery);
        await sectors.AddAsync(sector);

        var handler = new CreateGraveHandler(cemeteries, sectors, graves, spatial);

        var result = await handler.HandleAsync(new CreateGraveCommand(cemetery.Id, sector.Id, "J-01", Inside()));

        Assert.True(result.IsFailure);
        Assert.Equal("cemetery.grave.outside-cemetery", result.Error.Code);
    }

    [Fact]
    public async Task GraveRequiresSectorFromSameCemetery()
    {
        var cemeteries = new InMemoryCemeteryRepository();
        var sectors = new InMemoryCemeterySectorRepository();
        var graves = new InMemoryGraveRepository();

        var cemetery = CemeteryFactory.New("A");
        var otherCemetery = CemeteryFactory.New("B");
        await cemeteries.AddAsync(cemetery);
        await cemeteries.AddAsync(otherCemetery);

        var sector = CemeteryFactory.NewSector(otherCemetery);
        await sectors.AddAsync(sector);

        var handler = new CreateGraveHandler(cemeteries, sectors, graves, new FakeSpatialOperations());

        var result = await handler.HandleAsync(new CreateGraveCommand(cemetery.Id, sector.Id, "J-01", Inside()));

        Assert.True(result.IsFailure);
        Assert.Equal("cemetery.grave.sector-mismatch", result.Error.Code);
    }

    [Fact]
    public async Task DuplicateGraveCodeInSectorIsRejected()
    {
        var cemeteries = new InMemoryCemeteryRepository();
        var sectors = new InMemoryCemeterySectorRepository();
        var graves = new InMemoryGraveRepository();

        var cemetery = CemeteryFactory.New();
        await cemeteries.AddAsync(cemetery);
        var sector = CemeteryFactory.NewSector(cemetery);
        await sectors.AddAsync(sector);

        var handler = new CreateGraveHandler(cemeteries, sectors, graves, new FakeSpatialOperations());
        await handler.HandleAsync(new CreateGraveCommand(cemetery.Id, sector.Id, "J-01", Inside()));

        var result = await handler.HandleAsync(new CreateGraveCommand(cemetery.Id, sector.Id, "j-01", Inside()));

        Assert.True(result.IsFailure);
        Assert.Equal("cemetery.grave.duplicated", result.Error.Code);
    }

    [Fact]
    public async Task BurialThenSecondBurialFailsThroughHandler()
    {
        var cemeteries = new InMemoryCemeteryRepository();
        var sectors = new InMemoryCemeterySectorRepository();
        var graves = new InMemoryGraveRepository();

        var cemetery = CemeteryFactory.New();
        await cemeteries.AddAsync(cemetery);
        var sector = CemeteryFactory.NewSector(cemetery);
        await sectors.AddAsync(sector);
        var grave = CemeteryFactory.NewGrave(cemetery, sector);
        await graves.AddAsync(grave);

        var handler = new RegisterBurialHandler(graves);
        var first = await handler.HandleAsync(new RegisterBurialCommand(
            grave.Id,
            "Maria",
            new DateOnly(2026, 1, 1)));

        var second = await handler.HandleAsync(new RegisterBurialCommand(
            grave.Id,
            "Joao",
            new DateOnly(2026, 2, 1)));

        Assert.True(first.IsSuccess);
        Assert.True(second.IsFailure);
        Assert.Equal("cemetery.grave.occupied", second.Error.Code);
    }
}