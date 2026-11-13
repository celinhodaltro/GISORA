using GISORA.Domain.Common;

namespace GISORA.Domain.Geospatial;

public sealed class LineString : Geometry
{
    public LineString(CoordinateSequence sequence)
    {
        Sequence = Guard.NotNull(sequence, nameof(sequence));
        Guard.Condition(Sequence.Count >= 2, "Linha exige ao menos 2 coordenadas.", "geospatial.linestring.minimum");
    }

    public CoordinateSequence Sequence { get; }

    public override GeometryType Type => GeometryType.LineString;

    public override SpatialReference SpatialReference => Sequence.SpatialReference;

    public override IReadOnlyList<Coordinate> Coordinates => Sequence.Coordinates;

    public override BoundingBox GetBoundingBox() => BoundingBox.FromCoordinates(Sequence.Coordinates);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Sequence;
    }
}