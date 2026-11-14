using GISORA.Application.Geospatial;
using GISORA.Domain.Geospatial;

namespace GISORA.UnitTests.Geospatial;

public class GeometryDiagnosticsTests
{
    [Fact]
    public void RejectsOpenRing()
    {
        Coordinate[] ring =
        [
            Geo.Point(0, 0),
            Geo.Point(10, 0),
            Geo.Point(10, 10),
            Geo.Point(0, 10),
        ];

        var issues = GeometryDiagnostics.InspectRing(ring);

        Assert.Contains(issues, issue => issue.Code == "geometry.ring.open");
    }

    [Fact]
    public void RejectsRingWithLessThanFourCoordinates()
    {
        Coordinate[] ring =
        [
            Geo.Point(0, 0),
            Geo.Point(10, 0),
            Geo.Point(0, 0),
        ];

        var issues = GeometryDiagnostics.InspectRing(ring);

        Assert.Contains(issues, issue => issue.Code == "geometry.ring.minimum");
    }

    [Fact]
    public void RejectsDuplicatedVertex()
    {
        Coordinate[] ring =
        [
            Geo.Point(0, 0),
            Geo.Point(10, 0),
            Geo.Point(10, 0),
            Geo.Point(10, 10),
            Geo.Point(0, 10),
            Geo.Point(0, 0),
        ];

        var issues = GeometryDiagnostics.InspectRing(ring);

        var issue = Assert.Single(issues, issue => issue.Code == "geometry.duplicate-vertex");
        Assert.Equal(10, issue.Location!.X);
        Assert.Equal(0, issue.Location.Y);
    }

    [Fact]
    public void RejectsSelfIntersectingRingAndReportsLocation()
    {
        Coordinate[] ring =
        [
            Geo.Point(0, 0),
            Geo.Point(10, 10),
            Geo.Point(10, 0),
            Geo.Point(0, 10),
            Geo.Point(0, 0),
        ];

        var issues = GeometryDiagnostics.InspectRing(ring);

        var issue = Assert.Single(issues, issue => issue.Code == "geometry.self-intersection");
        Assert.NotNull(issue.Location);
        Assert.Equal(5, issue.Location!.X, 6);
        Assert.Equal(5, issue.Location.Y, 6);
    }

    [Fact]
    public void AcceptsSimpleRing()
    {
        Coordinate[] ring =
        [
            Geo.Point(0, 0),
            Geo.Point(10, 0),
            Geo.Point(10, 10),
            Geo.Point(0, 10),
            Geo.Point(0, 0),
        ];

        var issues = GeometryDiagnostics.InspectRing(ring);

        Assert.Empty(issues);
    }

    [Fact]
    public void InspectsEveryRingOfPolygon()
    {
        var polygon = new Polygon([
            new CoordinateSequence([
                Geo.Point(0, 0),
                Geo.Point(30, 0),
                Geo.Point(30, 30),
                Geo.Point(0, 30),
                Geo.Point(0, 0),
            ]),
            new CoordinateSequence([
                Geo.Point(10, 10),
                Geo.Point(20, 10),
                Geo.Point(20, 10),
                Geo.Point(10, 10),
            ]),
        ]);

        var issues = GeometryDiagnostics.Inspect(polygon);

        Assert.Contains(issues, issue => issue.Code == "geometry.duplicate-vertex");
        Assert.Contains(issues, issue => issue.Message.Contains("Anel interno 1", StringComparison.Ordinal));
    }
}