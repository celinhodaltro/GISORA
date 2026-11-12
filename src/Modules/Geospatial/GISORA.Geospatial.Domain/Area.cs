using GISORA.BuildingBlocks.Domain;

namespace GISORA.Geospatial.Domain;

public enum AreaUnit
{
    SquareMeters,
    Hectares,
}

/// <summary>
/// Area sempre nao negativa, com unidade explicita. Conversao m2 &lt;-&gt; ha sem perda para valores decimais.
/// </summary>
public sealed class Area : ValueObject
{
    private const decimal SquareMetersPerHectare = 10_000m;

    private Area(decimal value, AreaUnit unit)
    {
        Guard.Condition(value >= 0, "Area nao pode ser negativa.", "geospatial.area.negative");

        Value = value;
        Unit = unit;
    }

    public decimal Value { get; }

    public AreaUnit Unit { get; }

    public decimal SquareMeters => Unit == AreaUnit.Hectares ? Value * SquareMetersPerHectare : Value;

    public decimal Hectares => Unit == AreaUnit.SquareMeters ? Value / SquareMetersPerHectare : Value;

    public static Area Zero => new(0m, AreaUnit.SquareMeters);

    public static Area FromSquareMeters(decimal value) => new(value, AreaUnit.SquareMeters);

    public static Area FromHectares(decimal value) => new(value, AreaUnit.Hectares);

    public override string ToString()
        => Unit == AreaUnit.Hectares ? $"{Value} ha" : $"{Value} m2";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        // Igualdade pelo valor em metros quadrados: 1 ha == 10000 m2.
        yield return SquareMeters;
    }
}