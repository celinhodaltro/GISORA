using GISORA.Domain.Common;

namespace GISORA.Domain.Geospatial;

public sealed class Point : Geometry
{
    public Point(Coordinate coordinate)
    {
        Coordinate = Guard.NotNull(coordinate, nameof(coordinate));
    }

    public Coordinate Coordinate { get; }

    public override GeometryType Type => GeometryType.Point;

    public override SpatialReference SpatialReference => Coordinate.SpatialReference;

    public override IReadOnlyList<Coordinate> Coordinates => [Coordinate];

    public override BoundingBox GetBoundingBox()
        => new(Coordinate.X, Coordinate.Y, Coordinate.X, Coordinate.Y, SpatialReference);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Coordinate;
    }
}