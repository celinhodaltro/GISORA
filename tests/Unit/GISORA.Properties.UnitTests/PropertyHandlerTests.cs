using GISORA.Properties.Application;
using GISORA.Properties.Application.Commands;
using GISORA.Properties.Application.Queries;
using GISORA.Properties.Domain;
using GISORA.Properties.UnitTests.Fakes;

namespace GISORA.Properties.UnitTests;

public class PropertyHandlerTests
{
    private static CreatePropertyCommand ValidCommand() => new(
        "001",
        "002",
        "003",
        null,
        Guid.CreateVersion7(),
        "10",
        Guid.CreateVersion7(),
        500m,
        null);

    [Fact]
    public async Task CreatePropertySucceeds()
    {
        var repository = new InMemoryPropertyRepository();
        var handler = new CreatePropertyHandler(repository);

        var result = await handler.HandleAsync(ValidCommand());

        Assert.True(result.IsSuccess);
        Assert.NotNull(await repository.FindAsync(result.Value));
    }

    [Fact]
    public async Task CreatePropertyRejectsDuplicatedRegistration()
    {
        var handler = new CreatePropertyHandler(new InMemoryPropertyRepository());
        await handler.HandleAsync(ValidCommand());

        var result = await handler.HandleAsync(ValidCommand());

        Assert.True(result.IsFailure);
        Assert.Equal("properties.registration.duplicated", result.Error.Code);
    }

    [Fact]
    public async Task CreatePropertyTranslatesInvariantIntoValidationError()
    {
        var handler = new CreatePropertyHandler(new InMemoryPropertyRepository());

        var result = await handler.HandleAsync(ValidCommand() with { LandAreaSquareMeters = 0m });

        Assert.True(result.IsFailure);
        Assert.Equal("properties.land-area", result.Error.Code);
    }

    [Fact]
    public async Task UpdateGeometryFailsWhenPropertyIsMissing()
    {
        var handler = new UpdatePropertyGeometryHandler(new InMemoryPropertyRepository());

        var result = await handler.HandleAsync(new UpdatePropertyGeometryCommand(
            PropertyId.New(),
            PropertyFactory.Square(0, 0, 5),
            null));

        Assert.True(result.IsFailure);
        Assert.Equal("properties.not-found", result.Error.Code);
    }

    [Fact]
    public async Task UpdateGeometryPersistsHistory()
    {
        var repository = new InMemoryPropertyRepository();
        var property = PropertyFactory.Create();
        await repository.AddAsync(property);
        var handler = new UpdatePropertyGeometryHandler(repository);

        var result = await handler.HandleAsync(new UpdatePropertyGeometryCommand(
            property.Id,
            PropertyFactory.Square(0, 0, 5),
            "retificacao"));

        Assert.True(result.IsSuccess);
        Assert.Single(property.History);
    }

    [Fact]
    public async Task AddOwnershipFailsWhenTaxpayerIsMissing()
    {
        var properties = new InMemoryPropertyRepository();
        var property = PropertyFactory.Create();
        await properties.AddAsync(property);
        var handler = new AddOwnershipHandler(properties, new InMemoryTaxpayerRepository());

        var result = await handler.HandleAsync(new AddOwnershipCommand(
            property.Id,
            TaxpayerId.New(),
            100m,
            new DateOnly(2026, 1, 1)));

        Assert.True(result.IsFailure);
        Assert.Equal("properties.taxpayer.not-found", result.Error.Code);
    }

    [Fact]
    public async Task AddOwnershipSucceedsForExistingTaxpayer()
    {
        var properties = new InMemoryPropertyRepository();
        var taxpayers = new InMemoryTaxpayerRepository();
        var property = PropertyFactory.Create();
        await properties.AddAsync(property);
        var taxpayer = Taxpayer.Register("Maria", DocumentNumber.Parse("52998224725"));
        await taxpayers.AddAsync(taxpayer);
        var handler = new AddOwnershipHandler(properties, taxpayers);

        var result = await handler.HandleAsync(new AddOwnershipCommand(
            property.Id,
            taxpayer.Id,
            100m,
            new DateOnly(2026, 1, 1)));

        Assert.True(result.IsSuccess);
        Assert.Equal(100m, property.OwnershipTotal);
    }

    [Fact]
    public async Task GetPropertyReturnsProjection()
    {
        var repository = new InMemoryPropertyRepository();
        var property = PropertyFactory.Create();
        property.AddBuilding(BuildingType.Residential, GISORA.Geospatial.Domain.Area.FromSquareMeters(100m), 1);
        await repository.AddAsync(property);
        var handler = new GetPropertyHandler(repository);

        var result = await handler.HandleAsync(new GetPropertyQuery(property.Id));

        Assert.True(result.IsSuccess);
        Assert.Equal("001.002.003", result.Value.Registration);
        Assert.Single(result.Value.Buildings);
        Assert.Equal(100m, result.Value.BuiltAreaSquareMeters);
    }

    [Fact]
    public async Task SearchClampsPagination()
    {
        var queries = new FakePropertyQueries();
        var handler = new SearchPropertiesHandler(queries);

        var result = await handler.HandleAsync(new SearchPropertiesQuery(new PropertyFilter
        {
            PageNumber = 0,
            PageSize = 1_000,
        }));

        Assert.True(result.IsSuccess);
        Assert.Equal(1, queries.LastFilter!.PageNumber);
        Assert.Equal(200, queries.LastFilter.PageSize);
    }
}