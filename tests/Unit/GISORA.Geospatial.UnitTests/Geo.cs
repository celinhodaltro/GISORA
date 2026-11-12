using GISORA.Geospatial.Domain;

namespace GISORA.Geospatial.UnitTests;

internal static class Geo
{
    public static readonly SpatialReference Wgs84 = SpatialReference.Wgs84;

    public static readonly SpatialReference Sirgas2000 = SpatialReference.FromEpsg(4674);

    public static Coordinate Point(double x, double y, SpatialReference? reference = null)
        => new(x, y, reference ?? Wgs84);

    public static CoordinateSequence ClosedRing(params (double X, double Y)[] points)
        => new(points.Select(point => Point(point.X, point.Y)));

    public static Polygon Square(double x, double y, double size)
        => new([
            ClosedRing(
                (x, y),
                (x + size, y),
                (x + size, y + size),
                (x, y + size),
                (x, y)),
        ]);
}