using System.Globalization;

using GISORA.Domain.Common;

namespace GISORA.Domain.Territory;

public sealed class IbgeCode : ValueObject
{
    private static readonly HashSet<int> ValidStateCodes =
    [
        11, 12, 13, 14, 15, 16, 17,
        21, 22, 23, 24, 25, 26, 27, 28, 29,
        31, 32, 33, 35,
        41, 42, 43,
        50, 51, 52, 53,
    ];

    private IbgeCode(string value) => Value = value;

    public string Value { get; }

    public int StateCode => int.Parse(Value[..2], CultureInfo.InvariantCulture);

    public static IbgeCode Parse(string? value)
    {
        var trimmed = Guard.NotNullOrWhiteSpace(value, nameof(value)).Trim();

        Guard.Condition(
            trimmed.Length == 7 && trimmed.All(char.IsAsciiDigit),
            "Codigo IBGE deve conter exatamente 7 digitos.",
            "territory.ibge.format");

        var stateCode = int.Parse(trimmed[..2], CultureInfo.InvariantCulture);
        Guard.Condition(
            ValidStateCodes.Contains(stateCode),
            "Codigo IBGE com unidade federativa invalida.",
            "territory.ibge.state");

        return new IbgeCode(trimmed);
    }

    public override string ToString() => Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}