using GISORA.BuildingBlocks.Application;
using GISORA.Geospatial.Domain;
using GISORA.Properties.Domain;

namespace GISORA.Properties.Application;

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