using GISORA.Geospatial.Domain;
using GISORA.Rural.Domain;

namespace GISORA.Infrastructure.LocalStorage.Persistence;

internal sealed class LegalReserveDocument
{
    public GeometryDocument Boundary { get; init; } = null!;

    public decimal AreaSquareMeters { get; init; }

    public LegalReserve ToDomain() => LegalReserve.Define(
        (Polygon)Boundary.ToDomain(),
        Area.FromSquareMeters(AreaSquareMeters));

    public static LegalReserveDocument FromDomain(LegalReserve reserve) => new()
    {
        Boundary = GeometryDocument.FromDomain(reserve.Boundary),
        AreaSquareMeters = reserve.Area.SquareMeters,
    };
}

internal sealed class PreservationAreaDocument
{
    public Guid Id { get; init; }

    public string Label { get; init; } = string.Empty;

    public GeometryDocument Boundary { get; init; } = null!;

    public decimal AreaSquareMeters { get; init; }

    public PreservationArea ToDomain() => PreservationArea.Rehydrate(
        new PreservationAreaId(Id),
        Label,
        (Polygon)Boundary.ToDomain(),
        Area.FromSquareMeters(AreaSquareMeters));

    public static PreservationAreaDocument FromDomain(PreservationArea area) => new()
    {
        Id = area.Id.Value,
        Label = area.Label,
        Boundary = GeometryDocument.FromDomain(area.Boundary),
        AreaSquareMeters = area.Area.SquareMeters,
    };
}

internal sealed class RuralPropertyDocument
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public GeometryDocument Boundary { get; init; } = null!;

    public decimal AreaSquareMeters { get; init; }

    public RuralUsage Usage { get; init; }

    public Guid? MunicipalityId { get; init; }

    public string? CarCode { get; init; }

    public string? SigefCode { get; init; }

    public string? SnciCode { get; init; }

    public string SourceKind { get; init; } = nameof(RuralSourceKind.Manual);

    public string SourceCrs { get; init; } = "EPSG:4326";

    public DateOnly SourceImportedAt { get; init; }

    public LegalReserveDocument? LegalReserve { get; init; }

    public List<PreservationAreaDocument> PreservationAreas { get; init; } = [];

    public RuralProperty ToDomain() => RuralProperty.Rehydrate(
        new RuralPropertyId(Id),
        Name,
        (Polygon)Boundary.ToDomain(),
        Area.FromSquareMeters(AreaSquareMeters),
        Usage,
        MunicipalityId,
        CarCode,
        SigefCode,
        SnciCode,
        new RuralSource(SourceKind, SourceCrs, SourceImportedAt),
        LegalReserve?.ToDomain(),
        PreservationAreas.Select(area => area.ToDomain()));

    public static RuralPropertyDocument FromDomain(RuralProperty property) => new()
    {
        Id = property.Id.Value,
        Name = property.Name,
        Boundary = GeometryDocument.FromDomain(property.Boundary),
        AreaSquareMeters = property.Area.SquareMeters,
        Usage = property.Usage,
        MunicipalityId = property.MunicipalityId,
        CarCode = property.CarCode,
        SigefCode = property.SigefCode,
        SnciCode = property.SnciCode,
        SourceKind = property.Source.Kind,
        SourceCrs = property.Source.Crs,
        SourceImportedAt = property.Source.ImportedAt,
        LegalReserve = property.LegalReserve is null ? null : LegalReserveDocument.FromDomain(property.LegalReserve),
        PreservationAreas = property.PreservationAreas
            .Select(area => PreservationAreaDocument.FromDomain(area))
            .ToList(),
    };
}

internal sealed record RuralPropertyIndexEntry
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string NameSearch { get; init; } = string.Empty;

    public RuralUsage Usage { get; init; }

    public string? CarCode { get; init; }

    public decimal AreaSquareMeters { get; init; }

    public decimal ConstrainedSquareMeters { get; init; }

    public int PreservationAreaCount { get; init; }

    public string SourceKind { get; init; } = string.Empty;
}