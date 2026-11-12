using GISORA.Geospatial.Domain;

namespace GISORA.Infrastructure.Geospatial.UnitTests;

public class GeoJsonMapperTests
{
    private static readonly SpatialReference Wgs84 = SpatialReference.Wgs84;

    private static Polygon Square()
        => new([
            new CoordinateSequence([
                new Coordinate(0, 0, Wgs84),
                new Coordinate(10, 0, Wgs84),
                new Coordinate(10, 10, Wgs84),
                new Coordinate(0, 10, Wgs84),
                new Coordinate(0, 0, Wgs84),
            ]),
        ]);

    [Fact]
    public void RoundTripsPoint()
    {
        var point = new Point(new Coordinate(-35.25, -7.95, Wgs84, 120));

        var geometry = GeoJsonMapper.FromGeoJson(GeoJsonMapper.ToGeoJson(point), Wgs84);

        var result = Assert.IsType<Point>(geometry);
        Assert.Equal(-35.25, result.Coordinate.X);
        Assert.Equal(-7.95, result.Coordinate.Y);
        Assert.Equal(120, result.Coordinate.Z);
        Assert.Equal(Wgs84, result.SpatialReference);
    }

    [Fact]
    public void RoundTripsLineString()
    {
        var line = new LineString(new CoordinateSequence([
            new Coordinate(0, 0, Wgs84),
            new Coordinate(5, 5, Wgs84),
        ]));

        var geometry = GeoJsonMapper.FromGeoJson(GeoJsonMapper.ToGeoJson(line), Wgs84);

        var result = Assert.IsType<LineString>(geometry);
        Assert.Equal(2, result.Coordinates.Count);
    }

    [Fact]
    public void RoundTripsPolygonWithHole()
    {
        var polygon = new Polygon([
            new CoordinateSequence([
                new Coordinate(0, 0, Wgs84),
                new Coordinate(10, 0, Wgs84),
                new Coordinate(10, 10, Wgs84),
                new Coordinate(0, 10, Wgs84),
                new Coordinate(0, 0, Wgs84),
            ]),
            new CoordinateSequence([
                new Coordinate(2, 2, Wgs84),
                new Coordinate(4, 2, Wgs84),
                new Coordinate(4, 4, Wgs84),
                new Coordinate(2, 2, Wgs84),
            ]),
        ]);

        var geometry = GeoJsonMapper.FromGeoJson(GeoJsonMapper.ToGeoJson(polygon), Wgs84);

        var result = Assert.IsType<Polygon>(geometry);
        Assert.Equal(2, result.Rings.Count);
        Assert.Equal(5, result.ExteriorRing.Count);
    }

    [Fact]
    public void RoundTripsMultiPolygon()
    {
        var multiPolygon = new MultiPolygon([Square(), Square()]);

        var geometry = GeoJsonMapper.FromGeoJson(GeoJsonMapper.ToGeoJson(multiPolygon), Wgs84);

        var result = Assert.IsType<MultiPolygon>(geometry);
        Assert.Equal(2, result.Polygons.Count);
    }

    [Fact]
    public void SerializesGeoJsonWithTypeAndCoordinates()
    {
        var json = GeoJsonMapper.ToGeoJson(Square());

        Assert.Contains("\"type\":\"Polygon\"", json, StringComparison.Ordinal);
        Assert.Contains("\"coordinates\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public void RejectsUnsupportedType()
    {
        var exception = Assert.Throws<GeospatialException>(() =>
            GeoJsonMapper.FromGeoJson("{\"type\":\"GeometryCollection\",\"coordinates\":[]}", Wgs84));

        Assert.Contains("GeometryCollection", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RejectsInvalidJson()
    {
        Assert.Throws<GeospatialException>(() => GeoJsonMapper.FromGeoJson("{\"coordinates\":[]}", Wgs84));
    }
}