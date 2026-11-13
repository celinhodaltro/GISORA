using GISORA.Application.Common;
using GISORA.Domain.Geospatial;
using GISORA.Domain.Properties;

namespace GISORA.Application.Properties;

public sealed record PropertyFilter
{
    public string? RegistrationContains { get; init; }

    public Guid? NeighborhoodId { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 25;
}

public sealed record PropertyGeometryView(
    PropertyId Id,
    string Registration,
    Geometry? Geometry,
    decimal LandAreaSquareMeters);

public interface IPropertyQueries
{
    Task<PagedResult<PropertyListItem>> SearchAsync(PropertyFilter filter, CancellationToken cancellationToken = default);

    Task<PropertyGeometryView?> GetGeometryAsync(PropertyId id, CancellationToken cancellationToken = default);
}