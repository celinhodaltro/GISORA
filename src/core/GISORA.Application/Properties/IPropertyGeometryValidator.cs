using GISORA.Application.Geospatial;
using GISORA.Domain.Geospatial;
using GISORA.Domain.Properties;

namespace GISORA.Application.Properties;

public interface IPropertyGeometryValidator
{
    Task<GeometryValidationResult> ValidateAsync(
        Polygon geometry,
        PropertyId? excludedPropertyId = null,
        CancellationToken cancellationToken = default);
}