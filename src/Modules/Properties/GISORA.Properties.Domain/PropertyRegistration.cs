using GISORA.BuildingBlocks.Domain;

namespace GISORA.Properties.Domain;

public sealed class PropertyRegistration : ValueObject
{
    private PropertyRegistration(string sector, string block, string lot, string? unit)
    {
        Sector = Normalize(sector, nameof(sector));
        Block = Normalize(block, nameof(block));
        Lot = Normalize(lot, nameof(lot));
        Unit = unit is null ? null : Normalize(unit, nameof(unit));
    }

    public string Sector { get; }

    public string Block { get; }

    public string Lot { get; }

    public string? Unit { get; }

    public string Formatted => Unit is null
        ? $"{Sector}.{Block}.{Lot}"
        : $"{Sector}.{Block}.{Lot}.{Unit}";

    public static PropertyRegistration Create(string sector, string block, string lot, string? unit = null)
        => new(sector, block, lot, unit);

    public override string ToString() => Formatted;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Sector;
        yield return Block;
        yield return Lot;
        yield return Unit;
    }

    private static string Normalize(string value, string parameterName)
    {
        var normalized = Guard.NotNullOrWhiteSpace(value, parameterName).Trim().ToUpperInvariant();

        Guard.Condition(
            normalized.Length <= 10 && normalized.All(character => char.IsAsciiLetterOrDigit(character) || character == '-'),
            "Componente da inscricao imobiliaria aceita apenas letras, digitos e hifen.",
            "properties.registration.format");

        return normalized;
    }
}