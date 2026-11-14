using GISORA.Application.Geospatial;
using GISORA.Application.Properties;
using GISORA.Application.Properties.Commands;
using GISORA.Application.Properties.Queries;
using GISORA.Domain.Properties;
using GISORA.UnitTests.Properties.Fakes;

namespace GISORA.UnitTests.Properties;

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
        var handler = new CreatePropertyHandler(repository, new FakePropertyGeometryValidator());

        var result = await handler.HandleAsync(ValidCommand());

        Assert.True(result.IsSuccess);
        Assert.NotNull(await repository.FindAsync(result.Value));
    }

    [Fact]
    public async Task CreatePropertyRejectsDuplicatedRegistration()
    {
        var handler = new CreatePropertyHandler(new InMemoryPropertyRepository(), new FakePropertyGeometryValidator());
        await handler.HandleAsync(ValidCommand());

        var result = await handler.HandleAsync(ValidCommand());

        Assert.True(result.IsFailure);
        Assert.Equal("properties.registration.duplicated", result.Error.Code);
    }

    [Fact]
    public async Task CreatePropertyTranslatesInvariantIntoValidationError()
    {
        var handler = new CreatePropertyHandler(new InMemoryPropertyRepository(), new FakePropertyGeometryValidator());

        var result = await handler.HandleAsync(ValidCommand() with { LandAreaSquareMeters = 0m });

        Assert.True(result.IsFailure);
        Assert.Equal("properties.land-area", result.Error.Code);
    }

    [Fact]
    public async Task UpdateGeometryFailsWhenPropertyIsMissing()
    {
        var handler = new UpdatePropertyGeometryHandler(new InMemoryPropertyRepository(), new FakePropertyGeometryValidator());

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
        var handler = new UpdatePropertyGeometryHandler(repository, new FakePropertyGeometryValidator());

        var result = await handler.HandleAsync(new UpdatePropertyGeometryCommand(
            property.Id,
            PropertyFactory.Square(0, 0, 5),
            "retificacao"));

        Assert.True(result.IsSuccess);
        Assert.Single(property.History);
    }

    [Fact]
    public async Task UpdateGeometryRejectsOverlapWithoutChangingProperty()
    {
        var repository = new InMemoryPropertyRepository();
        var property = PropertyFactory.Create();
        await repository.AddAsync(property);
        var validator = new FakePropertyGeometryValidator
        {
            Result = GeometryValidationResult.Invalid(
                new GeometryIssue(PropertyGeometryValidator.OverlapCode, "Geometria sobrepoe o imovel 001.002.004.")),
        };
        var handler = new UpdatePropertyGeometryHandler(repository, validator);

        var result = await handler.HandleAsync(new UpdatePropertyGeometryCommand(
            property.Id,
            PropertyFactory.Square(0, 0, 5),
            "retificacao"));

        Assert.True(result.IsFailure);
        Assert.Equal(PropertyGeometryValidator.OverlapCode, result.Error.Code);
        Assert.Null(property.Geometry);
        Assert.Empty(property.History);
        Assert.Equal(property.Id, validator.LastExcludedPropertyId);
    }

    [Fact]
    public async Task UpdateGeometryUsesPropertyIdAsExclusion()
    {
        var repository = new InMemoryPropertyRepository();
        var property = PropertyFactory.Create();
        await repository.AddAsync(property);
        var validator = new FakePropertyGeometryValidator();
        var handler = new UpdatePropertyGeometryHandler(repository, validator);

        var result = await handler.HandleAsync(new UpdatePropertyGeometryCommand(
            property.Id,
            PropertyFactory.Square(0, 0, 5),
            null));

        Assert.True(result.IsSuccess);
        Assert.Equal(property.Id, validator.LastExcludedPropertyId);
    }

    [Fact]
    public async Task CreatePropertyRejectsInvalidGeometryWithoutPersisting()
    {
        var repository = new InMemoryPropertyRepository();
        var validator = new FakePropertyGeometryValidator
        {
            Result = GeometryValidationResult.Invalid(
                new GeometryIssue("geometry.ring.open", "Anel externo deve estar fechado.")),
        };
        var handler = new CreatePropertyHandler(repository, validator);

        var result = await handler.HandleAsync(ValidCommand() with { Geometry = PropertyFactory.Square(0, 0, 5) });

        Assert.True(result.IsFailure);
        Assert.Equal("properties.geometry.invalid", result.Error.Code);
        Assert.Empty(repository.Items);
    }

    [Fact]
    public async Task ClearGeometryFailsWhenPropertyIsMissing()
    {
        var handler = new ClearPropertyGeometryHandler(new InMemoryPropertyRepository());

        var result = await handler.HandleAsync(new ClearPropertyGeometryCommand(PropertyId.New(), "remocao"));

        Assert.True(result.IsFailure);
        Assert.Equal("properties.not-found", result.Error.Code);
    }

    [Fact]
    public async Task ClearGeometryPersistsRemovalAndHistory()
    {
        var repository = new InMemoryPropertyRepository();
        var property = PropertyFactory.Create();
        property.ChangeGeometry(PropertyFactory.Square(0, 0, 5), null);
        await repository.AddAsync(property);
        var handler = new ClearPropertyGeometryHandler(repository);

        var result = await handler.HandleAsync(new ClearPropertyGeometryCommand(property.Id, "remocao"));

        Assert.True(result.IsSuccess);
        Assert.Null(property.Geometry);
        var entry = property.History.Last();
        Assert.Equal("geometry", entry.Field);
        Assert.NotNull(entry.PreviousValue);
        Assert.Null(entry.CurrentValue);
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
        property.AddBuilding(BuildingType.Residential, GISORA.Domain.Geospatial.Area.FromSquareMeters(100m), 1);
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