using GISORA.Application.Geospatial;
using GISORA.Application.Properties;
using GISORA.Domain.Properties;
using GISORA.UnitTests.Properties.Fakes;

namespace GISORA.UnitTests.Properties;

public class PropertyGeometryValidatorTests
{
    [Fact]
    public async Task ReportsOverlapWithRegistrationAndLocation()
    {
        var other = PropertyFactory.Create();
        var maps = new InMemoryPropertyMapQueries();
        maps.Items.Add(new PropertyMapItem(
            other.Id,
            "001.002.004",
            500m,
            0m,
            PropertyFactory.Square(0, 0, 10)));
        var spatial = new FakeSpatialOperations
        {
            Intersection = (_, _) => PropertyFactory.Square(0, 0, 5),
            AreaSquareMeters = 25m,
        };
        var validator = new PropertyGeometryValidator(new FakeGeometryValidator(), maps, spatial);

        var result = await validator.ValidateAsync(PropertyFactory.Square(0, 0, 10), PropertyId.New());

        Assert.False(result.IsValid);
        var issue = Assert.Single(result.Issues);
        Assert.Equal(PropertyGeometryValidator.OverlapCode, issue.Code);
        Assert.Contains("001.002.004", issue.Message);
        Assert.NotNull(issue.Location);
    }

    [Fact]
    public async Task IgnoresThePropertyBeingEdited()
    {
        var property = PropertyFactory.Create();
        var maps = new InMemoryPropertyMapQueries();
        maps.Items.Add(new PropertyMapItem(
            property.Id,
            property.Registration.Formatted,
            500m,
            0m,
            PropertyFactory.Square(0, 0, 10)));
        var spatial = new FakeSpatialOperations
        {
            Intersection = (_, _) => PropertyFactory.Square(0, 0, 5),
            AreaSquareMeters = 25m,
        };
        var validator = new PropertyGeometryValidator(new FakeGeometryValidator(), maps, spatial);

        var result = await validator.ValidateAsync(PropertyFactory.Square(0, 0, 10), property.Id);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task AcceptsTouchingGeometryWithoutArea()
    {
        var other = PropertyFactory.Create();
        var maps = new InMemoryPropertyMapQueries();
        maps.Items.Add(new PropertyMapItem(
            other.Id,
            other.Registration.Formatted,
            500m,
            0m,
            PropertyFactory.Square(0, 10, 10)));
        var validator = new PropertyGeometryValidator(new FakeGeometryValidator(), maps, new FakeSpatialOperations());

        var result = await validator.ValidateAsync(PropertyFactory.Square(0, 0, 10), PropertyId.New());

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task AcceptsOverlapBelowTolerance()
    {
        var other = PropertyFactory.Create();
        var maps = new InMemoryPropertyMapQueries();
        maps.Items.Add(new PropertyMapItem(
            other.Id,
            other.Registration.Formatted,
            500m,
            0m,
            PropertyFactory.Square(0, 0, 10)));
        var spatial = new FakeSpatialOperations
        {
            Intersection = (_, _) => PropertyFactory.Square(0, 0, 1),
            AreaSquareMeters = 0.5m,
        };
        var validator = new PropertyGeometryValidator(new FakeGeometryValidator(), maps, spatial);

        var result = await validator.ValidateAsync(PropertyFactory.Square(0, 0, 10), PropertyId.New());

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task SkipsOverlapCheckWhenStructureIsInvalid()
    {
        var other = PropertyFactory.Create();
        var maps = new InMemoryPropertyMapQueries();
        maps.Items.Add(new PropertyMapItem(
            other.Id,
            other.Registration.Formatted,
            500m,
            0m,
            PropertyFactory.Square(0, 0, 10)));
        var structural = new FakeGeometryValidator
        {
            Result = GeometryValidationResult.Invalid(new GeometryIssue("geometry.self-intersection", "Auto-intersecao.")),
        };
        var validator = new PropertyGeometryValidator(structural, maps, new FakeSpatialOperations());

        var result = await validator.ValidateAsync(PropertyFactory.Square(0, 0, 10), PropertyId.New());

        Assert.False(result.IsValid);
        Assert.Equal("geometry.self-intersection", Assert.Single(result.Issues).Code);
    }
}