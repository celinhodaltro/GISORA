using GISORA.Domain.Geospatial;

namespace GISORA.Application.Geospatial;

public interface IGeometryValidator
{
    Task<GeometryValidationResult> ValidateAsync(Geometry geometry, CancellationToken cancellationToken = default);
}