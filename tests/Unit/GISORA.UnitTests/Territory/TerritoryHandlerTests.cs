using GISORA.Application.Territory;
using GISORA.Application.Territory.Commands;
using GISORA.Application.Territory.Queries;
using GISORA.Domain.Geospatial;
using GISORA.Domain.Territory;
using GISORA.UnitTests.Territory.Fakes;

namespace GISORA.UnitTests.Territory;

public class TerritoryHandlerTests
{
    [Fact]
    public async Task CreateMunicipalitySucceeds()
    {
        var repository = new InMemoryMunicipalityRepository();
        var handler = new CreateMunicipalityHandler(repository);

        var result = await handler.HandleAsync(new CreateMunicipalityCommand("Passira", "2611903"));

        Assert.True(result.IsSuccess);
        Assert.Equal(1, repository.AddCount);
    }

    [Fact]
    public async Task CreateMunicipalityRejectsDuplicatedIbgeCode()
    {
        var repository = new InMemoryMunicipalityRepository();
        var handler = new CreateMunicipalityHandler(repository);
        await handler.HandleAsync(new CreateMunicipalityCommand("Passira", "2611903"));

        var result = await handler.HandleAsync(new CreateMunicipalityCommand("Outro", "2611903"));

        Assert.True(result.IsFailure);
        Assert.Equal("territory.municipality.duplicated", result.Error.Code);
        Assert.Equal(1, repository.AddCount);
    }

    [Fact]
    public async Task CreateMunicipalityTranslatesDomainExceptionToValidationError()
    {
        var handler = new CreateMunicipalityHandler(new InMemoryMunicipalityRepository());

        var result = await handler.HandleAsync(new CreateMunicipalityCommand("Passira", "abc"));

        Assert.True(result.IsFailure);
        Assert.Equal("territory.ibge.format", result.Error.Code);
    }

    [Fact]
    public async Task CreateUrbanBlockRejectsDuplicatedCodeInSector()
    {
        var repository = new InMemoryUrbanBlockRepository();
        var handler = new CreateUrbanBlockHandler(repository);
        var sector = FiscalSectorId.New();
        await handler.HandleAsync(new CreateUrbanBlockCommand(sector, "Q-01"));

        var result = await handler.HandleAsync(new CreateUrbanBlockCommand(sector, "Q-01"));

        Assert.True(result.IsFailure);
        Assert.Equal("territory.urbanblock.duplicated", result.Error.Code);
    }

    [Fact]
    public async Task CreateUrbanBlockAllowsSameCodeInDifferentSector()
    {
        var repository = new InMemoryUrbanBlockRepository();
        var handler = new CreateUrbanBlockHandler(repository);
        await handler.HandleAsync(new CreateUrbanBlockCommand(FiscalSectorId.New(), "Q-01"));

        var result = await handler.HandleAsync(new CreateUrbanBlockCommand(FiscalSectorId.New(), "Q-01"));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task AddBlockFaceFailsWhenBlockDoesNotExist()
    {
        var handler = new AddBlockFaceHandler(new InMemoryUrbanBlockRepository());

        var result = await handler.HandleAsync(new AddBlockFaceCommand(
            UrbanBlockId.New(),
            Guid.CreateVersion7(),
            SimpleLine(),
            10m));

        Assert.True(result.IsFailure);
        Assert.Equal("territory.urbanblock.not-found", result.Error.Code);
    }

    [Fact]
    public async Task GetUrbanBlockReturnsProjection()
    {
        var repository = new InMemoryUrbanBlockRepository();
        var block = UrbanBlock.Create(FiscalSectorId.New(), "Q-02");
        block.AddFace(Guid.CreateVersion7(), SimpleLine(), Distance.FromMeters(12));
        await repository.AddAsync(block);
        var handler = new GetUrbanBlockHandler(repository);

        var result = await handler.HandleAsync(new GetUrbanBlockQuery(block.Id));

        Assert.True(result.IsSuccess);
        Assert.Equal("Q-02", result.Value.Code);
        Assert.Single(result.Value.Faces);
        Assert.Equal(12m, result.Value.Faces[0].FrontageMeters);
    }

    [Fact]
    public async Task GetUrbanBlockFailsWhenMissing()
    {
        var handler = new GetUrbanBlockHandler(new InMemoryUrbanBlockRepository());

        var result = await handler.HandleAsync(new GetUrbanBlockQuery(UrbanBlockId.New()));

        Assert.True(result.IsFailure);
        Assert.Equal("territory.urbanblock.not-found", result.Error.Code);
    }

    [Fact]
    public async Task SearchClampsPageParameters()
    {
        var queries = new FakeUrbanBlockQueries();
        var handler = new SearchUrbanBlocksHandler(queries);

        var result = await handler.HandleAsync(new SearchUrbanBlocksQuery(new UrbanBlockFilter
        {
            PageNumber = -5,
            PageSize = 5_000,
        }));

        Assert.True(result.IsSuccess);
        Assert.Equal(1, queries.LastFilter!.PageNumber);
        Assert.Equal(200, queries.LastFilter.PageSize);
    }

    private static LineString SimpleLine()
        => new(new CoordinateSequence([
            new Coordinate(0, 0, SpatialReference.Wgs84),
            new Coordinate(1, 0, SpatialReference.Wgs84),
        ]));
}