using GISORA.PublicFacilities.Domain;

namespace GISORA.Infrastructure.LocalStorage.Persistence;

internal sealed class FacilityDocument
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public FacilityType Type { get; init; }

    public Guid? NeighborhoodId { get; init; }

    public GeometryDocument? Location { get; init; }

    public PublicFacility ToDomain() => PublicFacility.Rehydrate(
        new PublicFacilityId(Id),
        Name,
        Type,
        NeighborhoodId,
        Location is null ? null : (GISORA.Geospatial.Domain.Point)Location.ToDomain());

    public static FacilityDocument FromDomain(PublicFacility facility) => new()
    {
        Id = facility.Id.Value,
        Name = facility.Name,
        Type = facility.Type,
        NeighborhoodId = facility.NeighborhoodId,
        Location = facility.Location is null ? null : GeometryDocument.FromDomain(facility.Location),
    };
}

internal sealed record FacilityIndexEntry
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string NameSearch { get; init; } = string.Empty;

    public FacilityType Type { get; init; }

    public Guid? NeighborhoodId { get; init; }

    public bool HasLocation { get; init; }

    public static FacilityIndexEntry FromDomain(PublicFacility facility) => new()
    {
        Id = facility.Id.Value,
        Name = facility.Name,
        NameSearch = facility.Name.ToUpperInvariant(),
        Type = facility.Type,
        NeighborhoodId = facility.NeighborhoodId,
        HasLocation = facility.Location is not null,
    };
}