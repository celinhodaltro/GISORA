using GISORA.Domain.Geospatial;
using GISORA.Domain.Properties;

namespace GISORA.Application.Properties;

public sealed record PropertyMapItem(
    PropertyId Id,
    string Registration,
    decimal LandAreaSquareMeters,
    decimal BuiltAreaSquareMeters,
    Geometry Geometry);

public interface IPropertyMapQueries
{
    Task<IReadOnlyList<PropertyMapItem>> GetMapItemsAsync(CancellationToken cancellationToken = default);
}