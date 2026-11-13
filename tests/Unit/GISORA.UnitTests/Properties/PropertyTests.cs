using GISORA.Domain.Common;
using GISORA.Domain.Geospatial;
using GISORA.Domain.Properties;

namespace GISORA.UnitTests.Properties;

internal static class PropertyFactory
{
    public static Property Create(decimal landAreaSquareMeters = 500m)
        => Property.Register(
            PropertyRegistration.Create("001", "002", "003"),
            new Address(Guid.CreateVersion7(), "10", Guid.CreateVersion7()),
            Area.FromSquareMeters(landAreaSquareMeters));

    public static Polygon Square(double x, double y, double size)
        => new([
            new CoordinateSequence([
                new Coordinate(x, y, SpatialReference.Wgs84),
                new Coordinate(x + size, y, SpatialReference.Wgs84),
                new Coordinate(x + size, y + size, SpatialReference.Wgs84),
                new Coordinate(x, y + size, SpatialReference.Wgs84),
                new Coordinate(x, y, SpatialReference.Wgs84),
            ]),
        ]);
}

public class PropertyTests
{
    [Fact]
    public void RegisterRaisesEventAndTrimsRegistration()
    {
        var property = PropertyFactory.Create();

        var domainEvent = Assert.IsType<PropertyRegistered>(Assert.Single(property.DomainEvents));
        Assert.Equal("001.002.003", property.Registration.Formatted);
        Assert.Equal(property.Id, domainEvent.PropertyId);
    }

    [Fact]
    public void RejectsZeroLandArea()
    {
        var exception = Assert.Throws<DomainException>(() => Property.Register(
            PropertyRegistration.Create("001", "002", "003"),
            new Address(Guid.CreateVersion7(), "10", Guid.CreateVersion7()),
            Area.FromSquareMeters(0)));

        Assert.Equal("properties.land-area", exception.Code);
    }

    [Fact]
    public void BuiltAreaAccumulatesBuildings()
    {
        var property = PropertyFactory.Create(1_000m);

        property.AddBuilding(BuildingType.Residential, Area.FromSquareMeters(200m), 2);
        property.AddBuilding(BuildingType.Commercial, Area.FromSquareMeters(150m), 1);

        Assert.Equal(350m, property.BuiltArea.SquareMeters);
    }

    [Fact]
    public void RejectsBuiltAreaGreaterThanLandArea()
    {
        var property = PropertyFactory.Create(300m);

        var exception = Assert.Throws<DomainException>(() =>
            property.AddBuilding(BuildingType.Residential, Area.FromSquareMeters(300.01m), 1));

        Assert.Equal("properties.building.exceeds-land-area", exception.Code);
    }

    [Fact]
    public void ChangeGeometryRecordsHistory()
    {
        var property = PropertyFactory.Create();
        var original = PropertyFactory.Square(0, 0, 10);
        var updated = PropertyFactory.Square(0, 0, 12);
        property.ChangeGeometry(original, "cadastro inicial");
        property.DequeueDomainEvents();

        property.ChangeGeometry(updated, "retificacao");

        Assert.Equal(2, property.History.Count);
        var entry = property.History.Last();
        Assert.Equal("geometry", entry.Field);
        Assert.NotNull(entry.PreviousValue);
        Assert.NotNull(entry.CurrentValue);
        Assert.Single(property.DequeueDomainEvents().OfType<PropertyGeometryChanged>());
    }

    [Fact]
    public void ChangeGeometryToSameValueDoesNotRecordHistory()
    {
        var property = PropertyFactory.Create();
        var geometry = PropertyFactory.Square(0, 0, 10);
        property.ChangeGeometry(geometry, null);
        var historyCount = property.History.Count;

        property.ChangeGeometry(PropertyFactory.Square(0, 0, 10), null);

        Assert.Equal(historyCount, property.History.Count);
    }

    [Fact]
    public void OwnershipBalancesWithThreeShares()
    {
        var property = PropertyFactory.Create();

        property.AddOwnership(TaxpayerId.New(), OwnershipShare.FromPercentage(33.33m), new DateOnly(2026, 1, 1));
        property.AddOwnership(TaxpayerId.New(), OwnershipShare.FromPercentage(33.33m), new DateOnly(2026, 1, 1));
        property.AddOwnership(TaxpayerId.New(), OwnershipShare.FromPercentage(33.34m), new DateOnly(2026, 1, 1));

        property.EnsureOwnershipBalanced();

        Assert.Equal(100m, property.OwnershipTotal);
    }

    [Fact]
    public void UnbalancedOwnershipIsRejected()
    {
        var property = PropertyFactory.Create();
        property.AddOwnership(TaxpayerId.New(), OwnershipShare.FromPercentage(50m), new DateOnly(2026, 1, 1));

        var exception = Assert.Throws<DomainException>(property.EnsureOwnershipBalanced);

        Assert.Equal("properties.ownership.unbalanced", exception.Code);
    }

    [Fact]
    public void RejectsOwnershipTotalAboveHundred()
    {
        var property = PropertyFactory.Create();
        property.AddOwnership(TaxpayerId.New(), OwnershipShare.FromPercentage(60m), new DateOnly(2026, 1, 1));

        var exception = Assert.Throws<DomainException>(() =>
            property.AddOwnership(TaxpayerId.New(), OwnershipShare.FromPercentage(50m), new DateOnly(2026, 1, 1)));

        Assert.Equal("properties.ownership.exceeds-total", exception.Code);
    }

    [Fact]
    public void NoOwnershipIsBalanced()
    {
        PropertyFactory.Create().EnsureOwnershipBalanced();
    }

    [Fact]
    public void ShareMustBeWithinRange()
    {
        Assert.Throws<DomainException>(() => OwnershipShare.FromPercentage(0m));
        Assert.Throws<DomainException>(() => OwnershipShare.FromPercentage(100.01m));
    }
}