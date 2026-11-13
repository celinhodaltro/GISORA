using GISORA.Domain.Common;
using GISORA.Domain.Geospatial;

namespace GISORA.Domain.PublicFacilities;

public enum FacilityType
{
    School,
    HealthCenter,
    Hospital,
    Square,
    Park,
    PublicBuilding,
    Other,
}

public sealed record PublicFacilityId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static PublicFacilityId New() => new(Guid.CreateVersion7());
}

public sealed record PublicFacilityRegistered(PublicFacilityId FacilityId, string Name) : DomainEvent;

public sealed class PublicFacility : AggregateRoot<PublicFacilityId>
{
    private PublicFacility(PublicFacilityId id, string name, FacilityType type, Guid? neighborhoodId, Point? location)
        : base(id)
    {
        Name = ValidateName(name);
        Type = type;
        NeighborhoodId = neighborhoodId;
        Location = location;
    }

    public string Name { get; private set; }

    public FacilityType Type { get; private set; }

    public Guid? NeighborhoodId { get; private set; }

    public Point? Location { get; private set; }

    public static PublicFacility Register(string name, FacilityType type, Guid? neighborhoodId, Point? location)
    {
        var facility = new PublicFacility(PublicFacilityId.New(), name, type, neighborhoodId, location);
        facility.Raise(new PublicFacilityRegistered(facility.Id, facility.Name));

        return facility;
    }

    public void Rename(string name) => Name = ValidateName(name);

    public void Relocate(Point location) => Location = Guard.NotNull(location, nameof(location));

    internal static PublicFacility Rehydrate(PublicFacilityId id, string name, FacilityType type, Guid? neighborhoodId, Point? location)
        => new(id, name, type, neighborhoodId, location);

    private static string ValidateName(string name)
    {
        var normalized = Guard.NotNullOrWhiteSpace(name, nameof(name)).Trim();

        Guard.Condition(normalized.Length <= 120, "Nome do equipamento excede 120 caracteres.", "facilities.name-length");

        return normalized;
    }
}