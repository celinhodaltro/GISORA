using GISORA.BuildingBlocks.Domain;

namespace GISORA.Geospatial.Domain;

public abstract class Geometry : ValueObject
{
    public abstract GeometryType Type { get; }

    public abstract SpatialReference SpatialReference { get; }

    public abstract IReadOnlyList<Coordinate> Coordinates { get; }

    public abstract BoundingBox GetBoundingBox();
}