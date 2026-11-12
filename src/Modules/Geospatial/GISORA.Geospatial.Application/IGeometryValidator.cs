using GISORA.Geospatial.Domain;

namespace GISORA.Geospatial.Application;

/// <summary>
/// Valida consistencia topologica que o kernel estrutural nao resolve
/// (auto-interseccao, aneis sobrepostos, orientacao).
/// Implementacao real (Turf/PostGIS) entra na Fase 2 e no backend.
/// </summary>
public interface IGeometryValidator
{
    Task<GeometryValidationResult> ValidateAsync(Geometry geometry, CancellationToken cancellationToken = default);
}