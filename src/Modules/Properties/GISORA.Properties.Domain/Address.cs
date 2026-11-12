using GISORA.BuildingBlocks.Domain;

namespace GISORA.Properties.Domain;

public sealed class Address : ValueObject
{
    public Address(Guid streetId, string number, Guid neighborhoodId, string? complement = null)
    {
        Guard.NotEmpty(streetId, nameof(streetId));
        Guard.NotEmpty(neighborhoodId, nameof(neighborhoodId));

        StreetId = streetId;
        Number = Guard.NotNullOrWhiteSpace(number, nameof(number)).Trim();
        NeighborhoodId = neighborhoodId;
        Complement = string.IsNullOrWhiteSpace(complement) ? null : complement.Trim();
    }

    public Guid StreetId { get; }

    public string Number { get; }

    public Guid NeighborhoodId { get; }

    public string? Complement { get; }

    public override string ToString()
        => Complement is null ? $"{StreetId}/{Number}" : $"{StreetId}/{Number} - {Complement}";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return StreetId;
        yield return Number;
        yield return NeighborhoodId;
        yield return Complement;
    }
}