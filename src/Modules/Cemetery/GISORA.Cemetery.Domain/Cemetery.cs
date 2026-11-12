using GISORA.BuildingBlocks.Domain;
using GISORA.Geospatial.Domain;

namespace GISORA.Cemetery.Domain;

public sealed record CemeteryId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static CemeteryId New() => new(Guid.CreateVersion7());
}

public sealed record CemeterySectorId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static CemeterySectorId New() => new(Guid.CreateVersion7());
}

public sealed record GraveId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static GraveId New() => new(Guid.CreateVersion7());
}

public sealed record CemeteryRegistered(CemeteryId CemeteryId, string Name) : DomainEvent;

public sealed record CemeterySectorCreated(CemeterySectorId SectorId, CemeteryId CemeteryId, string Code) : DomainEvent;

public sealed record GraveCreated(GraveId GraveId, CemeterySectorId SectorId, string Code) : DomainEvent;

public sealed record BurialRegistered(GraveId GraveId, Guid BurialId, string PersonName) : DomainEvent;

public sealed record BurialExhumed(GraveId GraveId, Guid BurialId) : DomainEvent;

public sealed class Cemetery : AggregateRoot<CemeteryId>
{
    private Cemetery(CemeteryId id, string name, Polygon boundary, Area area, Guid? neighborhoodId)
        : base(id)
    {
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name)).Trim();
        Boundary = Guard.NotNull(boundary, nameof(boundary));
        Area = Guard.NotNull(area, nameof(area));
        NeighborhoodId = neighborhoodId;

        Guard.Condition(area.SquareMeters > 0, "Area do cemiterio deve ser maior que zero.", "cemetery.area");
    }

    public string Name { get; private set; }

    public Polygon Boundary { get; private set; }

    public Area Area { get; private set; }

    public Guid? NeighborhoodId { get; private set; }

    public static Cemetery Register(string name, Polygon boundary, Area area, Guid? neighborhoodId = null)
    {
        var cemetery = new Cemetery(CemeteryId.New(), name, boundary, area, neighborhoodId);
        cemetery.Raise(new CemeteryRegistered(cemetery.Id, cemetery.Name));

        return cemetery;
    }

    public void Rename(string name) => Name = Guard.NotNullOrWhiteSpace(name, nameof(name)).Trim();

    public void RedefineBoundary(Polygon boundary, Area area)
    {
        Boundary = Guard.NotNull(boundary, nameof(boundary));
        Area = Guard.NotNull(area, nameof(area));

        Guard.Condition(area.SquareMeters > 0, "Area do cemiterio deve ser maior que zero.", "cemetery.area");
    }

    internal static Cemetery Rehydrate(CemeteryId id, string name, Polygon boundary, Area area, Guid? neighborhoodId)
        => new(id, name, boundary, area, neighborhoodId);
}

public sealed class CemeterySector : AggregateRoot<CemeterySectorId>
{
    private CemeterySector(CemeterySectorId id, CemeteryId cemeteryId, string code)
        : base(id)
    {
        CemeteryId = Guard.NotNull(cemeteryId, nameof(cemeteryId));
        Code = Guard.NotNullOrWhiteSpace(code, nameof(code)).Trim();
    }

    public CemeteryId CemeteryId { get; }

    public string Code { get; private set; }

    public static CemeterySector Create(CemeteryId cemeteryId, string code)
    {
        var sector = new CemeterySector(CemeterySectorId.New(), cemeteryId, code);
        sector.Raise(new CemeterySectorCreated(sector.Id, cemeteryId, sector.Code));

        return sector;
    }

    public void Rename(string code) => Code = Guard.NotNullOrWhiteSpace(code, nameof(code)).Trim();

    internal static CemeterySector Rehydrate(CemeterySectorId id, CemeteryId cemeteryId, string code)
        => new(id, cemeteryId, code);
}