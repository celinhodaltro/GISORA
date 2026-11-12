using GISORA.BuildingBlocks.Domain;
using GISORA.Geospatial.Domain;

namespace GISORA.Geospatial.UnitTests;

public class GeometryTests
{
    [Fact]
    public void LineStringRequiresAtLeastTwoCoordinates()
    {
        Assert.Throws<DomainException>(() => new LineString(new CoordinateSequence([Geo.Point(0, 0)])));
    }

    [Fact]
    public void PolygonRequiresAtLeastOneRing()
    {
        Assert.Throws<DomainException>(() => new Polygon([]));
    }

    [Fact]
    public void PolygonRejectsOpenRing()
    {
        var open = Geo.ClosedRing((0, 0), (1, 0), (1, 1), (0, 1));

        var exception = Assert.Throws<DomainException>(() => new Polygon([open]));

        Assert.Equal("geospatial.polygon.ring-open", exception.Code);
    }

    [Fact]
    public void PolygonRejectsRingWithLessThanFourCoordinates()
    {
        var triangle = Geo.ClosedRing((0, 0), (1, 0), (0, 0));

        var exception = Assert.Throws<DomainException>(() => new Polygon([triangle]));

        Assert.Equal("geospatial.polygon.ring-minimum", exception.Code);
    }

    [Fact]
    public void PolygonExposesExteriorAndInteriorRings()
    {
        var exterior = Geo.ClosedRing((0, 0), (10, 0), (10, 10), (0, 10), (0, 0));
        var hole = Geo.ClosedRing((2, 2), (4, 2), (4, 4), (2, 2));

        var polygon = new Polygon([exterior, hole]);

        Assert.Equal(exterior, polygon.ExteriorRing);
        Assert.Single(polygon.InteriorRings);
        Assert.Equal(9, polygon.Coordinates.Count);
    }

    [Fact]
    public void MultiPolygonRequiresAtLeastOnePolygon()
    {
        Assert.Throws<DomainException>(() => new MultiPolygon([]));
    }

    [Fact]
    public void MultiPolygonRejectsMixedSpatialReferences()
    {
        var wgs84 = Geo.Square(0, 0, 1);
        var sirgas = new Polygon([
            new CoordinateSequence([
                Geo.Point(0, 0, Geo.Sirgas2000),
                Geo.Point(1, 0, Geo.Sirgas2000),
                Geo.Point(0, 1, Geo.Sirgas2000),
                Geo.Point(0, 0, Geo.Sirgas2000),
            ]),
        ]);

        Assert.Throws<DomainException>(() => new MultiPolygon([wgs84, sirgas]));
    }

    [Fact]
    public void GeometryExposesItsType()
    {
        Assert.Equal(GeometryType.Point, new Point(Geo.Point(1, 2)).Type);
        Assert.Equal(GeometryType.Polygon, Geo.Square(0, 0, 1).Type);
        Assert.Equal(
            GeometryType.LineString,
            new LineString(new CoordinateSequence([Geo.Point(0, 0), Geo.Point(1, 1)])).Type);
    }
}