using GISORA.BuildingBlocks.Domain;

namespace GISORA.Geospatial.Domain;

public sealed class BoundingBox : ValueObject
{
    public BoundingBox(double minX, double minY, double maxX, double maxY, SpatialReference spatialReference)
    {
        Guard.NotNull(spatialReference, nameof(spatialReference));
        Guard.Condition(double.IsFinite(minX) && double.IsFinite(minY) && double.IsFinite(maxX) && double.IsFinite(maxY),
            "Bounding box exige numeros finitos.", "geospatial.bbox.finite");
        Guard.Condition(minX <= maxX && minY <= maxY,
            "Bounding box invalida: minimo maior que o maximo.", "geospatial.bbox.order");

        MinX = minX;
        MinY = minY;
        MaxX = maxX;
        MaxY = maxY;
        SpatialReference = spatialReference;
    }

    public double MinX { get; }

    public double MinY { get; }

    public double MaxX { get; }

    public double MaxY { get; }

    public SpatialReference SpatialReference { get; }

    public static BoundingBox FromCoordinates(IEnumerable<Coordinate> coordinates)
    {
        ArgumentNullException.ThrowIfNull(coordinates);

        var list = coordinates.ToArray();
        Guard.Condition(list.Length > 0, "Bounding box exige ao menos uma coordenada.", "geospatial.bbox.empty");

        var reference = list[0].SpatialReference;
        Guard.Condition(
            list.All(coordinate => coordinate.SpatialReference == reference),
            "Coordenadas do bounding box devem usar o mesmo sistema de referencia.",
            "geospatial.bbox.srid");

        return new BoundingBox(
            list.Min(coordinate => coordinate.X),
            list.Min(coordinate => coordinate.Y),
            list.Max(coordinate => coordinate.X),
            list.Max(coordinate => coordinate.Y),
            reference);
    }

    public bool Contains(Coordinate coordinate)
    {
        Guard.NotNull(coordinate, nameof(coordinate));
        Guard.Condition(coordinate.SpatialReference == SpatialReference,
            "Comparacao exige o mesmo sistema de referencia.", "geospatial.bbox.srid");

        return coordinate.X >= MinX && coordinate.X <= MaxX
               && coordinate.Y >= MinY && coordinate.Y <= MaxY;
    }

    public bool Intersects(BoundingBox other)
    {
        Guard.NotNull(other, nameof(other));
        Guard.Condition(other.SpatialReference == SpatialReference,
            "Comparacao exige o mesmo sistema de referencia.", "geospatial.bbox.srid");

        return MinX <= other.MaxX && MaxX >= other.MinX
               && MinY <= other.MaxY && MaxY >= other.MinY;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return MinX;
        yield return MinY;
        yield return MaxX;
        yield return MaxY;
        yield return SpatialReference;
    }
}