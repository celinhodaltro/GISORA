using GISORA.BuildingBlocks.Domain;
using GISORA.Geospatial.Domain;

namespace GISORA.Territory.Domain;

public sealed record MunicipalityRegistered(MunicipalityId MunicipalityId, string Name) : DomainEvent;

public sealed record UrbanBlockCreated(UrbanBlockId UrbanBlockId, FiscalSectorId FiscalSectorId, string Code) : DomainEvent;

public sealed record BlockFaceAdded(UrbanBlockId UrbanBlockId, Guid BlockFaceId) : DomainEvent;

public sealed class Municipality : AggregateRoot<MunicipalityId>
{
    private Municipality(MunicipalityId id, string name, IbgeCode ibgeCode, Geometry? boundary)
        : base(id)
    {
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name)).Trim();
        IbgeCode = Guard.NotNull(ibgeCode, nameof(ibgeCode));
        Boundary = boundary;
    }

    public string Name { get; }

    public IbgeCode IbgeCode { get; }

    public Geometry? Boundary { get; }

    public static Municipality Register(string name, IbgeCode ibgeCode, Geometry? boundary = null)
    {
        var municipality = new Municipality(MunicipalityId.New(), name, ibgeCode, boundary);
        municipality.Raise(new MunicipalityRegistered(municipality.Id, municipality.Name));

        return municipality;
    }

    internal static Municipality Rehydrate(MunicipalityId id, string name, IbgeCode ibgeCode, Geometry? boundary)
        => new(id, name, ibgeCode, boundary);
}