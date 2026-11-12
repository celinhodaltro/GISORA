using GISORA.BuildingBlocks.Domain;
using GISORA.Geospatial.Domain;

namespace GISORA.Geospatial.UnitTests;

public class CoordinateSequenceTests
{
    [Fact]
    public void SequenceCannotBeEmpty()
    {
        Assert.Throws<DomainException>(() => new CoordinateSequence([]));
    }

    [Fact]
    public void SequenceRejectsMixedSpatialReferences()
    {
        Assert.Throws<DomainException>(() => new CoordinateSequence([
            Geo.Point(1, 2),
            Geo.Point(3, 4, Geo.Sirgas2000),
        ]));
    }

    [Fact]
    public void ClosedRingIsDetected()
    {
        var ring = Geo.ClosedRing((0, 0), (1, 0), (1, 1), (0, 0));

        Assert.True(ring.IsClosed);
        Assert.Equal(4, ring.Count);
    }

    [Fact]
    public void OpenRingIsDetected()
    {
        var ring = Geo.ClosedRing((0, 0), (1, 0), (1, 1), (0, 1));

        Assert.False(ring.IsClosed);
    }
}