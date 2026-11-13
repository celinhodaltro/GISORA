using GISORA.Domain.Geospatial;

namespace GISORA.Infrastructure.Persistence.Documents;

internal sealed record CoordinateDocument(double X, double Y, double? Z)
{
    public Coordinate ToDomain(SpatialReference spatialReference) => new(X, Y, spatialReference, Z);
}

internal sealed record GeometryDocument(
    string Type,
    string Authority,
    int SridCode,
    CoordinateDocument[][] Rings,
    GeometryDocument[] Parts)
{
    private SpatialReference SpatialReference => GISORA.Domain.Geospatial.SpatialReference.From(Authority, SridCode);

    public Geometry ToDomain() => Type switch
    {
        nameof(GeometryType.Point) => new Point(ToSequence(Rings[0]).Coordinates[0]),
        nameof(GeometryType.LineString) => new LineString(ToSequence(Rings[0])),
        nameof(GeometryType.Polygon) => ToPolygon(),
        nameof(GeometryType.MultiPolygon) => new MultiPolygon(Parts.Select(part => part.ToPolygon())),
        _ => throw new StorageException($"Tipo de geometria desconhecido: {Type}."),
    };

    public static GeometryDocument FromDomain(Geometry geometry) => geometry switch
    {
        Point point => FromRings(
            nameof(GeometryType.Point),
            [[ToDocument(point.Coordinate)]],
            point.SpatialReference),
        LineString line => FromRings(
            nameof(GeometryType.LineString),
            [line.Sequence.Coordinates.Select(ToDocument).ToArray()],
            line.SpatialReference),
        Polygon polygon => FromRings(
            nameof(GeometryType.Polygon),
            polygon.Rings.Select(ring => ring.Coordinates.Select(ToDocument).ToArray()).ToArray(),
            polygon.SpatialReference),
        MultiPolygon multiPolygon => new GeometryDocument(
            nameof(GeometryType.MultiPolygon),
            multiPolygon.SpatialReference.Authority,
            multiPolygon.SpatialReference.Code,
            [],
            multiPolygon.Polygons.Select(FromDomain).ToArray()),
        _ => throw new StorageException($"Tipo de geometria nao suportado: {geometry.GetType().Name}."),
    };

    private Polygon ToPolygon() => new(Rings.Select(ToSequence));

    private CoordinateSequence ToSequence(CoordinateDocument[] coordinates)
    {
        var reference = SpatialReference;

        return new CoordinateSequence(coordinates.Select(coordinate => coordinate.ToDomain(reference)));
    }

    private static GeometryDocument FromRings(
        string type,
        CoordinateDocument[][] rings,
        SpatialReference reference)
        => new(type, reference.Authority, reference.Code, rings, []);

    private static CoordinateDocument ToDocument(Coordinate coordinate) => new(coordinate.X, coordinate.Y, coordinate.Z);
}