using GISORA.BuildingBlocks.Domain;
using GISORA.Geospatial.Domain;

namespace GISORA.Geospatial.UnitTests;

public class BoundingBoxTests
{
    [Fact]
    public void RejectsMinGreaterThanMax()
    {
        var exception = Assert.Throws<DomainException>(() => new BoundingBox(10, 0, 5, 10, Geo.Wgs84));

        Assert.Equal("geospatial.bbox.order", exception.Code);
    }

    [Fact]
    public void BuiltFromCoordinates()
    {
        var box = BoundingBox.FromCoordinates([Geo.Point(2, 5), Geo.Point(-1, 8), Geo.Point(4, 0)]);

        Assert.Equal(-1, box.MinX);
        Assert.Equal(0, box.MinY);
        Assert.Equal(4, box.MaxX);
        Assert.Equal(8, box.MaxY);
    }

    [Fact]
    public void ContainsCoordinateInsideAndOnBoundary()
    {
        var box = new BoundingBox(0, 0, 10, 10, Geo.Wgs84);

        Assert.True(box.Contains(Geo.Point(5, 5)));
        Assert.True(box.Contains(Geo.Point(0, 0)));
        Assert.False(box.Contains(Geo.Point(11, 5)));
    }

    [Fact]
    public void IntersectsOverlappingBoxes()
    {
        var left = new BoundingBox(0, 0, 5, 5, Geo.Wgs84);
        var right = new BoundingBox(4, 4, 10, 10, Geo.Wgs84);
        var apart = new BoundingBox(6, 6, 10, 10, Geo.Wgs84);

        Assert.True(left.Intersects(right));
        Assert.False(left.Intersects(apart));
    }

    [Fact]
    public void ComparisonRequiresSameSpatialReference()
    {
        var box = new BoundingBox(0, 0, 10, 10, Geo.Wgs84);
        var other = new BoundingBox(0, 0, 10, 10, Geo.Sirgas2000);

        Assert.Throws<DomainException>(() => box.Intersects(other));
    }
}