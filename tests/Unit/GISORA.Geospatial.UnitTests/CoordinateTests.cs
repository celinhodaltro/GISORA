using GISORA.BuildingBlocks.Domain;
using GISORA.Geospatial.Domain;

namespace GISORA.Geospatial.UnitTests;

public class CoordinateTests
{
    [Fact]
    public void CoordinateRequiresSpatialReference()
    {
        Assert.Throws<DomainException>(() => new Coordinate(1, 2, null!));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void CoordinateRejectsNonFiniteValues(double value)
    {
        Assert.Throws<DomainException>(() => new Coordinate(value, 0, Geo.Wgs84));
    }

    [Fact]
    public void CoordinatesWithSameValuesAndReferenceAreEqual()
    {
        Assert.Equal(Geo.Point(1, 2), Geo.Point(1, 2));
    }

    [Fact]
    public void CoordinatesWithDifferentReferenceAreNotEqual()
    {
        Assert.NotEqual(Geo.Point(1, 2), Geo.Point(1, 2, Geo.Sirgas2000));
    }

    [Fact]
    public void CoordinateKeepsOptionalAltitude()
    {
        var coordinate = new Coordinate(1, 2, Geo.Wgs84, 150.5);

        Assert.Equal(150.5, coordinate.Z);
    }
}