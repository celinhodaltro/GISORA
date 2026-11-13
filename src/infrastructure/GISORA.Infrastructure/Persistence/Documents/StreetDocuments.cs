using GISORA.Domain.Roads;

namespace GISORA.Infrastructure.Persistence.Documents;

internal sealed class StreetDocument
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public StreetType Type { get; init; }

    public Guid? NeighborhoodId { get; init; }

    public Street ToDomain() => Street.Rehydrate(new StreetId(Id), Name, Type, NeighborhoodId);

    public static StreetDocument FromDomain(Street street) => new()
    {
        Id = street.Id.Value,
        Name = street.Name,
        Type = street.Type,
        NeighborhoodId = street.NeighborhoodId,
    };
}

internal sealed record StreetIndexEntry
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string NameSearch { get; init; } = string.Empty;

    public StreetType Type { get; init; }

    public Guid? NeighborhoodId { get; init; }

    public static StreetIndexEntry FromDomain(Street street) => new()
    {
        Id = street.Id.Value,
        Name = street.Name,
        NameSearch = street.Name.ToUpperInvariant(),
        Type = street.Type,
        NeighborhoodId = street.NeighborhoodId,
    };
}