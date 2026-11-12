using GISORA.Geospatial.Domain;
using GISORA.Properties.Domain;

namespace GISORA.Properties.Application;

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