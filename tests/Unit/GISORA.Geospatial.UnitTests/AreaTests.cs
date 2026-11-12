using GISORA.BuildingBlocks.Domain;
using GISORA.Geospatial.Domain;

namespace GISORA.Geospatial.UnitTests;

public class AreaTests
{
    [Fact]
    public void RejectsNegativeArea()
    {
        var exception = Assert.Throws<DomainException>(() => Area.FromSquareMeters(-1m));

        Assert.Equal("geospatial.area.negative", exception.Code);
    }

    [Fact]
    public void ConvertsSquareMetersToHectares()
    {
        var area = Area.FromSquareMeters(15_000m);

        Assert.Equal(1.5m, area.Hectares);
        Assert.Equal(15_000m, area.SquareMeters);
    }

    [Fact]
    public void ConvertsHectaresToSquareMeters()
    {
        var area = Area.FromHectares(2.5m);

        Assert.Equal(25_000m, area.SquareMeters);
        Assert.Equal(2.5m, area.Hectares);
    }

    [Fact]
    public void RoundTripPreservesValue()
    {
        var original = Area.FromSquareMeters(12_345.678m);

        var roundTrip = Area.FromHectares(original.Hectares);

        Assert.Equal(original, roundTrip);
    }

    [Fact]
    public void EqualAreaInDifferentUnitsIsEqual()
    {
        Assert.Equal(Area.FromSquareMeters(10_000m), Area.FromHectares(1m));
    }

    [Fact]
    public void ZeroIsAllowed()
    {
        Assert.Equal(0m, Area.Zero.SquareMeters);
    }
}