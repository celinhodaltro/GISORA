using GISORA.BuildingBlocks.Domain;

namespace GISORA.Geospatial.Domain;

/// <summary>
/// Sequencia ordenada de coordenadas. Todas devem compartilhar o mesmo CRS.
/// </summary>
public sealed class CoordinateSequence : ValueObject
{
    private readonly Coordinate[] _coordinates;

    public CoordinateSequence(IEnumerable<Coordinate> coordinates)
    {
        ArgumentNullException.ThrowIfNull(coordinates);

        _coordinates = coordinates.ToArray();
        Guard.Condition(_coordinates.Length > 0, "Sequencia de coordenadas nao pode ser vazia.", "geospatial.sequence.empty");

        var reference = _coordinates[0].SpatialReference;
        Guard.Condition(
            _coordinates.All(coordinate => coordinate.SpatialReference == reference),
            "Todas as coordenadas devem usar o mesmo sistema de referencia.",
            "geospatial.sequence.srid");

        SpatialReference = reference;
    }

    public IReadOnlyList<Coordinate> Coordinates => _coordinates;

    public SpatialReference SpatialReference { get; }

    public int Count => _coordinates.Length;

    /// <summary>Anel fechado: primeira e ultima coordenadas iguais.</summary>
    public bool IsClosed => _coordinates.Length > 1 && _coordinates[0] == _coordinates[^1];

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        foreach (var coordinate in _coordinates)
        {
            yield return coordinate;
        }
    }
}