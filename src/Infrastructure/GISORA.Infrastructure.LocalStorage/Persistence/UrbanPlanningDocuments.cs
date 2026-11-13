using GISORA.Geospatial.Domain;
using GISORA.UrbanPlanning.Domain;

namespace GISORA.Infrastructure.LocalStorage.Persistence;

internal sealed class ZoneDocument
{
    public Guid Id { get; init; }

    public string Code { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public decimal? MaxOccupancyPercent { get; init; }

    public decimal? MaxHeightMeters { get; init; }

    public decimal? MinSetbackMeters { get; init; }

    public string? AllowedUses { get; init; }

    public GeometryDocument Boundary { get; init; } = null!;

    public decimal AreaSquareMeters { get; init; }

    public Zone ToDomain() => Zone.Rehydrate(
        new ZoneId(Id),
        Code,
        Name,
        ZoneParameters.Create(MaxOccupancyPercent, MaxHeightMeters, MinSetbackMeters, AllowedUses),
        (Polygon)Boundary.ToDomain(),
        Area.FromSquareMeters(AreaSquareMeters));

    public static ZoneDocument FromDomain(Zone zone) => new()
    {
        Id = zone.Id.Value,
        Code = zone.Code,
        Name = zone.Name,
        MaxOccupancyPercent = zone.Parameters.MaxOccupancyPercent,
        MaxHeightMeters = zone.Parameters.MaxHeightMeters,
        MinSetbackMeters = zone.Parameters.MinSetbackMeters,
        AllowedUses = zone.Parameters.AllowedUses,
        Boundary = GeometryDocument.FromDomain(zone.Boundary),
        AreaSquareMeters = zone.Area.SquareMeters,
    };
}

internal sealed class RestrictionDocument
{
    public Guid Id { get; init; }

    public RestrictionType Type { get; init; }

    public string Description { get; init; } = string.Empty;

    public GeometryDocument Boundary { get; init; } = null!;

    public decimal AreaSquareMeters { get; init; }

    public Restriction ToDomain() => Restriction.Rehydrate(
        new RestrictionId(Id),
        Type,
        Description,
        (Polygon)Boundary.ToDomain(),
        Area.FromSquareMeters(AreaSquareMeters));

    public static RestrictionDocument FromDomain(Restriction restriction) => new()
    {
        Id = restriction.Id.Value,
        Type = restriction.Type,
        Description = restriction.Description,
        Boundary = GeometryDocument.FromDomain(restriction.Boundary),
        AreaSquareMeters = restriction.Area.SquareMeters,
    };
}

internal sealed record ZoneIndexEntry
{
    public Guid Id { get; init; }

    public string Code { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public decimal? MaxOccupancyPercent { get; init; }

    public decimal? MaxHeightMeters { get; init; }

    public decimal? MinSetbackMeters { get; init; }

    public string? AllowedUses { get; init; }

    public decimal AreaSquareMeters { get; init; }
}