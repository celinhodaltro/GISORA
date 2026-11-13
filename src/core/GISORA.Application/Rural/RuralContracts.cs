using GISORA.Domain.Geospatial;
using GISORA.Domain.Rural;

namespace GISORA.Application.Rural;

public sealed record RuralPropertyListItem(
    RuralPropertyId Id,
    string Name,
    RuralUsage Usage,
    string? CarCode,
    decimal AreaHectares,
    decimal ConstrainedHectares,
    int PreservationAreaCount,
    string SourceKind);

public sealed record RuralPropertyMapItem(RuralPropertyId Id, string Name, Polygon Boundary);

public sealed record PreservationAreaListItem(
    PreservationAreaId Id,
    RuralPropertyId PropertyId,
    string Label,
    decimal AreaHectares);

public sealed record PreservationAreaMapItem(PreservationAreaId Id, RuralPropertyId PropertyId, string Label, Polygon Boundary);

public interface IRuralPropertyQueries
{
    Task<IReadOnlyList<RuralPropertyListItem>> ListAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RuralPropertyMapItem>> GetMapItemsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PreservationAreaListItem>> ListPreservationAreasAsync(
        RuralPropertyId? propertyId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PreservationAreaMapItem>> GetPreservationAreaMapItemsAsync(
        CancellationToken cancellationToken = default);
}