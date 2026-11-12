using GISORA.Geospatial.Domain;

namespace GISORA.Geospatial.Application;

/// <summary>
/// Operacoes espaciais delegadas. No cliente (Fase 2) via Turf.js; no servidor via PostGIS.
/// Consulta espacial nunca e implementada sobre LocalStorage.
/// </summary>
public interface ISpatialOperations
{
    Task<Area> AreaAsync(Geometry geometry, CancellationToken cancellationToken = default);

    Task<Distance> DistanceAsync(Geometry origin, Geometry target, CancellationToken cancellationToken = default);

    Task<bool> IntersectsAsync(Geometry left, Geometry right, CancellationToken cancellationToken = default);

    Task<bool> ContainsAsync(Geometry container, Geometry contained, CancellationToken cancellationToken = default);

    Task<Geometry?> IntersectionAsync(Geometry left, Geometry right, CancellationToken cancellationToken = default);
}