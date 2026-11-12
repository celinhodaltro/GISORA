using GISORA.Geospatial.Domain;

namespace GISORA.Geospatial.Application;

/// <summary>
/// Transforma geometria entre sistemas de referencia. Coordenadas originais sao preservadas:
/// a transformacao produz uma nova geometria, nunca muta a existente.
/// Implementacao: Proj4js (cliente) ou PostGIS (servidor).
/// </summary>
public interface ICoordinateTransformer
{
    Task<Geometry> TransformAsync(
        Geometry geometry,
        SpatialReference target,
        CancellationToken cancellationToken = default);
}