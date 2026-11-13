using GISORA.Domain.Geospatial;

namespace GISORA.Application.Geospatial;

public interface ICoordinateTransformer
{
    Task<Geometry> TransformAsync(
        Geometry geometry,
        SpatialReference target,
        CancellationToken cancellationToken = default);
}