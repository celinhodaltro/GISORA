using GISORA.BuildingBlocks.Domain;

namespace GISORA.Geospatial.Domain;

/// <summary>
/// Poligono com anel externo e aneis internos (buracos). Todos os aneis fechados e com o mesmo CRS.
/// </summary>
public sealed class Polygon : Geometry
{
    private readonly CoordinateSequence[] _rings;

    public Polygon(IEnumerable<CoordinateSequence> rings)
    {
        ArgumentNullException.ThrowIfNull(rings);

        _rings = rings.ToArray();
        Guard.Condition(_rings.Length > 0, "Poligono exige ao menos o anel externo.", "geospatial.polygon.empty");

        foreach (var ring in _rings)
        {
            ValidateRing(ring);
        }

        var reference = _rings[0].SpatialReference;
        Guard.Condition(
            _rings.All(ring => ring.SpatialReference == reference),
            "Todos os aneis devem usar o mesmo sistema de referencia.",
            "geospatial.polygon.srid");
    }

    public IReadOnlyList<CoordinateSequence> Rings => _rings;

    /// <summary>Anel externo (fronteira do poligono).</summary>
    public CoordinateSequence ExteriorRing => _rings[0];

    public IReadOnlyList<CoordinateSequence> InteriorRings => _rings[1..];

    public override GeometryType Type => GeometryType.Polygon;

    public override SpatialReference SpatialReference => _rings[0].SpatialReference;

    public override IReadOnlyList<Coordinate> Coordinates => _rings.SelectMany(ring => ring.Coordinates).ToArray();

    public override BoundingBox GetBoundingBox() => BoundingBox.FromCoordinates(Coordinates);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        foreach (var ring in _rings)
        {
            yield return ring;
        }
    }

    private static void ValidateRing(CoordinateSequence ring)
    {
        Guard.NotNull(ring, nameof(ring));
        Guard.Condition(ring.Count >= 4, "Anel deve ter ao menos 4 coordenadas.", "geospatial.polygon.ring-minimum");
        Guard.Condition(ring.IsClosed, "Anel deve estar fechado (primeira e ultima coordenadas iguais).", "geospatial.polygon.ring-open");
    }
}