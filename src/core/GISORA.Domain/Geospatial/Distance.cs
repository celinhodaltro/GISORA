using GISORA.Domain.Common;

namespace GISORA.Domain.Geospatial;

public sealed class Distance : ValueObject
{
    private Distance(decimal meters)
    {
        Guard.Condition(meters >= 0, "Distancia nao pode ser negativa.", "geospatial.distance.negative");
        Meters = meters;
    }

    public decimal Meters { get; }

    public decimal Kilometers => Meters / 1_000m;

    public static Distance Zero => new(0m);

    public static Distance FromMeters(decimal meters) => new(meters);

    public static Distance FromKilometers(decimal kilometers) => new(kilometers * 1_000m);

    public override string ToString() => $"{Meters} m";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Meters;
    }
}