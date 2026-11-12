using GISORA.Geospatial.Domain;

namespace GISORA.Geospatial.Application;

public interface IGeometryValidator
{
    Task<GeometryValidationResult> ValidateAsync(Geometry geometry, CancellationToken cancellationToken = default);
}