using GISORA.Domain.Common;

namespace GISORA.Domain.Geospatial;

public sealed class MultiPolygon : Geometry
{
    private readonly Polygon[] _polygons;

    public MultiPolygon(IEnumerable<Polygon> polygons)
    {
        ArgumentNullException.ThrowIfNull(polygons);

        _polygons = polygons.ToArray();
        Guard.Condition(_polygons.Length > 0, "Multi-poligono exige ao menos um poligono.", "geospatial.multipolygon.empty");

        var reference = _polygons[0].SpatialReference;
        Guard.Condition(
            _polygons.All(polygon => polygon.SpatialReference == reference),
            "Todos os poligonos devem usar o mesmo sistema de referencia.",
            "geospatial.multipolygon.srid");
    }

    public IReadOnlyList<Polygon> Polygons => _polygons;

    public override GeometryType Type => GeometryType.MultiPolygon;

    public override SpatialReference SpatialReference => _polygons[0].SpatialReference;

    public override IReadOnlyList<Coordinate> Coordinates => _polygons.SelectMany(polygon => polygon.Coordinates).ToArray();

    public override BoundingBox GetBoundingBox() => BoundingBox.FromCoordinates(Coordinates);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        foreach (var polygon in _polygons)
        {
            yield return polygon;
        }
    }
}