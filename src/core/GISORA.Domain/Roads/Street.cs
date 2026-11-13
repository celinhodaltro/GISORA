using GISORA.Domain.Common;

namespace GISORA.Domain.Roads;

public enum StreetType
{
    Street,
    Avenue,
    Lane,
    Highway,
    RuralRoad,
}

public sealed record StreetId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static StreetId New() => new(Guid.CreateVersion7());
}

public sealed record StreetRegistered(StreetId StreetId, string Name) : DomainEvent;

public sealed class Street : AggregateRoot<StreetId>
{
    private Street(StreetId id, string name, StreetType type, Guid? neighborhoodId)
        : base(id)
    {
        Name = ValidateName(name);
        Type = type;
        NeighborhoodId = neighborhoodId;
    }

    public string Name { get; private set; }

    public StreetType Type { get; private set; }

    public Guid? NeighborhoodId { get; private set; }

    public static Street Create(string name, StreetType type, Guid? neighborhoodId = null)
    {
        var street = new Street(StreetId.New(), name, type, neighborhoodId);
        street.Raise(new StreetRegistered(street.Id, street.Name));

        return street;
    }

    public void Rename(string name) => Name = ValidateName(name);

    public void ChangeType(StreetType type) => Type = type;

    public void ChangeNeighborhood(Guid? neighborhoodId) => NeighborhoodId = neighborhoodId;

    internal static Street Rehydrate(StreetId id, string name, StreetType type, Guid? neighborhoodId)
        => new(id, name, type, neighborhoodId);

    private static string ValidateName(string name)
    {
        var normalized = Guard.NotNullOrWhiteSpace(name, nameof(name)).Trim();

        Guard.Condition(normalized.Length <= 120, "Nome do logradouro excede 120 caracteres.", "roads.street.name-length");

        return normalized;
    }
}