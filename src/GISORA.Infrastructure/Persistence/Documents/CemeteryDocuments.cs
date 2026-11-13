using GISORA.Domain.Cemetery;

using CemeteryAggregate = GISORA.Domain.Cemetery.Cemetery;

namespace GISORA.Infrastructure.Persistence.Documents;

internal sealed class CemeteryDocument
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public GeometryDocument Boundary { get; init; } = null!;

    public decimal AreaSquareMeters { get; init; }

    public Guid? NeighborhoodId { get; init; }

    public CemeteryAggregate ToDomain() => CemeteryAggregate.Rehydrate(
        new CemeteryId(Id),
        Name,
        (GISORA.Domain.Geospatial.Polygon)Boundary.ToDomain(),
        GISORA.Domain.Geospatial.Area.FromSquareMeters(AreaSquareMeters),
        NeighborhoodId);

    public static CemeteryDocument FromDomain(CemeteryAggregate cemetery) => new()
    {
        Id = cemetery.Id.Value,
        Name = cemetery.Name,
        Boundary = GeometryDocument.FromDomain(cemetery.Boundary),
        AreaSquareMeters = cemetery.Area.SquareMeters,
        NeighborhoodId = cemetery.NeighborhoodId,
    };
}

internal sealed class CemeterySectorDocument
{
    public Guid Id { get; init; }

    public Guid CemeteryId { get; init; }

    public string Code { get; init; } = string.Empty;

    public CemeterySector ToDomain() => CemeterySector.Rehydrate(new CemeterySectorId(Id), new CemeteryId(CemeteryId), Code);

    public static CemeterySectorDocument FromDomain(CemeterySector sector) => new()
    {
        Id = sector.Id.Value,
        CemeteryId = sector.CemeteryId.Value,
        Code = sector.Code,
    };
}

internal sealed class BurialDocument
{
    public Guid Id { get; init; }

    public string PersonName { get; init; } = string.Empty;

    public DateOnly BuriedAt { get; init; }

    public DateOnly? ExhumedAt { get; init; }

    public Burial ToDomain() => Burial.Rehydrate(Id, PersonName, BuriedAt, ExhumedAt);
}

internal sealed class GraveDocument
{
    public Guid Id { get; init; }

    public Guid CemeteryId { get; init; }

    public Guid SectorId { get; init; }

    public string Code { get; init; } = string.Empty;

    public GeometryDocument Location { get; init; } = null!;

    public List<BurialDocument> Burials { get; init; } = [];

    public Grave ToDomain()
    {
        return Grave.Rehydrate(
            new GraveId(Id),
            new CemeteryId(CemeteryId),
            new CemeterySectorId(SectorId),
            Code,
            (GISORA.Domain.Geospatial.Point)Location.ToDomain(),
            Burials.Select(burial => burial.ToDomain()));
    }

    public static GraveDocument FromDomain(Grave grave) => new()
    {
        Id = grave.Id.Value,
        CemeteryId = grave.CemeteryId.Value,
        SectorId = grave.SectorId.Value,
        Code = grave.Code,
        Location = GeometryDocument.FromDomain(grave.Location),
        Burials = grave.Burials
            .Select(burial => new BurialDocument
            {
                Id = burial.Id,
                PersonName = burial.PersonName,
                BuriedAt = burial.BuriedAt,
                ExhumedAt = burial.ExhumedAt,
            })
            .ToList(),
    };
}

internal sealed record CemeteryIndexEntry
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public decimal AreaSquareMeters { get; init; }
}

internal sealed record CemeterySectorIndexEntry
{
    public Guid Id { get; init; }

    public Guid CemeteryId { get; init; }

    public string Code { get; init; } = string.Empty;

    public string CodeSearch { get; init; } = string.Empty;
}

internal sealed record GraveIndexEntry
{
    public Guid Id { get; init; }

    public Guid CemeteryId { get; init; }

    public Guid SectorId { get; init; }

    public string Code { get; init; } = string.Empty;

    public string CodeSearch { get; init; } = string.Empty;

    public bool IsOccupied { get; init; }

    public Guid? ActiveBurialId { get; init; }

    public string? ActiveBurialPerson { get; init; }
}