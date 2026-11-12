using System.Globalization;

using GISORA.BuildingBlocks.Domain;

namespace GISORA.Properties.Domain;

public enum DocumentType
{
    Cpf,
    Cnpj,
}

public sealed class DocumentNumber : ValueObject
{
    private DocumentNumber(string digits, DocumentType type)
    {
        Digits = digits;
        Type = type;
    }

    public string Digits { get; }

    public DocumentType Type { get; }

    public string Masked => Type == DocumentType.Cpf
        ? $"***.***.***-{Digits[9..11]}"
        : $"**.***.***/****-{Digits[12..14]}";

    public static DocumentNumber Parse(string? value)
    {
        var raw = Guard.NotNullOrWhiteSpace(value, nameof(value));
        var digits = new string(raw.Where(char.IsAsciiDigit).ToArray());

        Guard.Condition(
            digits.Length is 11 or 14,
            "Documento deve ter 11 (CPF) ou 14 (CNPJ) digitos.",
            "properties.document.length");

        Guard.Condition(
            digits.Distinct().Count() > 1,
            "Documento nao pode ter todos os digitos iguais.",
            "properties.document.repeated");

        if (digits.Length == 11)
        {
            Guard.Condition(IsValidCpf(digits), "CPF invalido.", "properties.document.invalid");

            return new DocumentNumber(digits, DocumentType.Cpf);
        }

        Guard.Condition(IsValidCnpj(digits), "CNPJ invalido.", "properties.document.invalid");

        return new DocumentNumber(digits, DocumentType.Cnpj);
    }

    public override string ToString() => Masked;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Digits;
    }

    private static bool IsValidCpf(string digits)
        => digits[9] == CpfCheckDigit(digits, 9, 10) && digits[10] == CpfCheckDigit(digits, 10, 11);

    private static bool IsValidCnpj(string digits)
    {
        int[] firstWeights = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] secondWeights = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

        return digits[12] == CnpjCheckDigit(digits, firstWeights)
               && digits[13] == CnpjCheckDigit(digits, secondWeights);
    }

    private static char CpfCheckDigit(string digits, int length, int weightStart)
    {
        var sum = 0;
        for (var index = 0; index < length; index++)
        {
            sum += (digits[index] - '0') * (weightStart - index);
        }

        return ToDigit(sum % 11);
    }

    private static char CnpjCheckDigit(string digits, int[] weights)
    {
        var sum = 0;
        for (var index = 0; index < weights.Length; index++)
        {
            sum += (digits[index] - '0') * weights[index];
        }

        return ToDigit(sum % 11);
    }

    private static char ToDigit(int remainder)
    {
        var digit = remainder < 2 ? 0 : 11 - remainder;

        return digit.ToString(CultureInfo.InvariantCulture)[0];
    }
}