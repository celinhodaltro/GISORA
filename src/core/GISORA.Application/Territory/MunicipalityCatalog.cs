using System.Globalization;
using System.Text;

namespace GISORA.Application.Territory;

public sealed record StateOption(string Abbreviation, string Name);

public sealed record MunicipalityOption(int Code, string Name, string StateAbbreviation);

public sealed class MunicipalityCatalogException : Exception
{
    public MunicipalityCatalogException(string message)
        : base(message)
    {
    }

    public MunicipalityCatalogException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

public interface IMunicipalityCatalog
{
    Task<IReadOnlyList<StateOption>> GetStatesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MunicipalityOption>> GetMunicipalitiesAsync(string stateAbbreviation, CancellationToken cancellationToken = default);
}

public static class MunicipalitySearch
{
    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    public static IReadOnlyList<MunicipalityOption> Filter(
        IEnumerable<MunicipalityOption> municipalities,
        string? term,
        int take = 8)
    {
        var normalizedTerm = Normalize(term);

        if (normalizedTerm.Length == 0)
        {
            return [];
        }

        return municipalities
            .Where(municipality => Normalize(municipality.Name).Contains(normalizedTerm, StringComparison.Ordinal))
            .OrderBy(municipality => Normalize(municipality.Name).StartsWith(normalizedTerm, StringComparison.Ordinal) ? 0 : 1)
            .ThenBy(municipality => municipality.Name, StringComparer.Ordinal)
            .Take(take)
            .ToArray();
    }
}