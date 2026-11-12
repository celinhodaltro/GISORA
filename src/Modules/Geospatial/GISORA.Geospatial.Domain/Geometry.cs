using GISORA.BuildingBlocks.Domain;

namespace GISORA.Geospatial.Domain;

/// <summary>
/// Geometria 2D/3D imutavel, sempre associada a um sistema de referencia.
/// Operacoes espaciais reais (area geodesica, interseccao, topologia) sao portas de Application.
/// </summary>
public abstract class Geometry : ValueObject
{
    public abstract GeometryType Type { get; }

    public abstract SpatialReference SpatialReference { get; }

    /// <summary>Coordenadas achatadas, util para bounding box e exportacao.</summary>
    public abstract IReadOnlyList<Coordinate> Coordinates { get; }

    public abstract BoundingBox GetBoundingBox();
}