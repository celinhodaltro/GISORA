using GISORA.Cemetery.Domain;
using GISORA.Geospatial.Domain;

namespace GISORA.Cemetery.Application;

public sealed record CemeteryListItem(
    CemeteryId Id,
    string Name,
    decimal AreaSquareMeters,
    int GraveCount);

public sealed record CemeterySectorListItem(
    CemeterySectorId Id,
    CemeteryId CemeteryId,
    string Code);

public sealed record GraveListItem(
    GraveId Id,
    CemeterySectorId SectorId,
    string Code,
    bool IsOccupied,
    Guid? ActiveBurialId,
    string? ActiveBurialPerson);

public sealed record CemeteryMapItem(CemeteryId Id, string Name, Polygon Boundary);

public sealed record GraveMapItem(GraveId Id, string Code, bool IsOccupied, Point Location);

public interface ICemeteryQueries
{
    Task<IReadOnlyList<CemeteryListItem>> ListCemeteriesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CemeterySectorListItem>> ListSectorsAsync(CemeteryId? cemeteryId = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GraveListItem>> ListGravesAsync(CemeterySectorId? sectorId = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CemeteryMapItem>> GetCemeteryMapItemsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GraveMapItem>> GetGraveMapItemsAsync(CancellationToken cancellationToken = default);
}