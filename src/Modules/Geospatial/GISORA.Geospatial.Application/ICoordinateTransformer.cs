using GISORA.Geospatial.Domain;

namespace GISORA.Geospatial.Application;

public interface ICoordinateTransformer
{
    Task<Geometry> TransformAsync(
        Geometry geometry,
        SpatialReference target,
        CancellationToken cancellationToken = default);
}