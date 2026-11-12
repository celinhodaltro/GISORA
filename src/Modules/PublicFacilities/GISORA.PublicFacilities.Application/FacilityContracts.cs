using GISORA.BuildingBlocks.Application;
using GISORA.Geospatial.Domain;
using GISORA.PublicFacilities.Domain;

namespace GISORA.PublicFacilities.Application;

public sealed record FacilityListItem(
    PublicFacilityId Id,
    string Name,
    FacilityType Type,
    Guid? NeighborhoodId,
    bool HasLocation);

public sealed record FacilityMapItem(
    PublicFacilityId Id,
    string Name,
    FacilityType Type,
    Point Location);

public sealed record FacilityFilter
{
    public string? NameContains { get; init; }

    public FacilityType? Type { get; init; }

    public Guid? NeighborhoodId { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 25;
}

public interface IFacilityQueries
{
    Task<PagedResult<FacilityListItem>> SearchAsync(FacilityFilter filter, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FacilityMapItem>> GetMapItemsAsync(CancellationToken cancellationToken = default);
}