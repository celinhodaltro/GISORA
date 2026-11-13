using GISORA.Domain.Geospatial;
using GISORA.Domain.UrbanPlanning;

namespace GISORA.Application.UrbanPlanning;

public sealed record ZoneListItem(
    ZoneId Id,
    string Code,
    string Name,
    decimal? MaxOccupancyPercent,
    decimal? MaxHeightMeters,
    decimal? MinSetbackMeters,
    string? AllowedUses,
    decimal AreaHectares);

public sealed record ZoneMapItem(ZoneId Id, string Code, string Name, Polygon Boundary);

public sealed record RestrictionMapItem(
    RestrictionId Id,
    RestrictionType Type,
    string Description,
    Polygon Boundary);

public sealed record IncidentRegulation(
    IReadOnlyList<ZoneListItem> Zones,
    IReadOnlyList<RestrictionMapItem> Restrictions);

public interface IRegulationQueries
{
    Task<IReadOnlyList<ZoneListItem>> ListZonesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ZoneMapItem>> GetZoneMapItemsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RestrictionMapItem>> GetRestrictionMapItemsAsync(CancellationToken cancellationToken = default);

    Task<IncidentRegulation> FindIncidentAsync(Geometry geometry, CancellationToken cancellationToken = default);
}