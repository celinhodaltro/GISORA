using GISORA.BuildingBlocks.Domain;
using GISORA.Geospatial.Domain;

namespace GISORA.Geospatial.UnitTests;

public class DistanceTests
{
    [Fact]
    public void RejectsNegativeDistance()
    {
        Assert.Throws<DomainException>(() => Distance.FromMeters(-0.1m));
    }

    [Fact]
    public void ConvertsKilometersToMeters()
    {
        var distance = Distance.FromKilometers(1.25m);

        Assert.Equal(1_250m, distance.Meters);
        Assert.Equal(1.25m, distance.Kilometers);
    }

    [Fact]
    public void EqualDistanceInDifferentUnitsIsEqual()
    {
        Assert.Equal(Distance.FromMeters(1_000m), Distance.FromKilometers(1m));
    }
}