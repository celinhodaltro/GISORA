using GISORA.Geospatial.Domain;

namespace GISORA.Geospatial.Application;

public interface ISpatialOperations
{
    Task<Area> AreaAsync(Geometry geometry, CancellationToken cancellationToken = default);

    Task<Distance> DistanceAsync(Geometry origin, Geometry target, CancellationToken cancellationToken = default);

    Task<bool> IntersectsAsync(Geometry left, Geometry right, CancellationToken cancellationToken = default);

    Task<bool> ContainsAsync(Geometry container, Geometry contained, CancellationToken cancellationToken = default);

    Task<Geometry?> IntersectionAsync(Geometry left, Geometry right, CancellationToken cancellationToken = default);
}