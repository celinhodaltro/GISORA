using System.Globalization;

using GISORA.Domain.Common;
using GISORA.Domain.Geospatial;

namespace GISORA.Application.Geospatial;

public sealed record CrsOption(int Code, string Name)
{
    public const string Authority = "EPSG";

    public string Identifier => $"{Authority}:{Code}";
}

public static class CrsCatalog
{
    public static readonly CrsOption Wgs84 = new(4326, "WGS 84 (geografico)");

    public static readonly CrsOption Sirgas2000Utm25S = new(31985, "SIRGAS 2000 / UTM zona 25S");

    public static IReadOnlyList<CrsOption> Supported { get; } =
    [
        Wgs84,
        new(31978, "SIRGAS 2000 / UTM zona 18S"),
        new(31979, "SIRGAS 2000 / UTM zona 19S"),
        new(31980, "SIRGAS 2000 / UTM zona 20S"),
        new(31981, "SIRGAS 2000 / UTM zona 21S"),
        new(31982, "SIRGAS 2000 / UTM zona 22S"),
        new(31983, "SIRGAS 2000 / UTM zona 23S"),
        new(31984, "SIRGAS 2000 / UTM zona 24S"),
        Sirgas2000Utm25S,
    ];

    public static SpatialReference Resolve(string? crs)
    {
        if (TryResolve(crs, out var reference, out var error) is false)
        {
            throw new DomainException(error ?? "Sistema de referencia invalido.", "geospatial.crs.invalid");
        }

        return reference;
    }

    public static bool TryResolve(string? crs, out SpatialReference reference, out string? error)
    {
        reference = SpatialReference.Wgs84;
        error = null;

        if (string.IsNullOrWhiteSpace(crs))
        {
            error = "Selecione um sistema de referencia (CRS).";
            return false;
        }

        var trimmed = crs.Trim();
        var parts = trimmed.Split(':', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2
            || string.Equals(parts[0], CrsOption.Authority, StringComparison.OrdinalIgnoreCase) is false
            || int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var code) is false)
        {
            error = $"CRS invalido: {trimmed}. Use o formato EPSG:<codigo>.";
            return false;
        }

        if (Supported.Any(option => option.Code == code) is false)
        {
            error = $"CRS nao suportado: EPSG:{code.ToString(CultureInfo.InvariantCulture)}. Suportados: EPSG:4326 e EPSG:31978 a EPSG:31985.";
            return false;
        }

        reference = SpatialReference.FromEpsg(code);

        return true;
    }

    public static string Describe(SpatialReference reference)
    {
        ArgumentNullException.ThrowIfNull(reference);

        var option = Supported.FirstOrDefault(candidate => candidate.Code == reference.Code);

        return option is null ? reference.ToString() : $"{reference} - {option.Name}";
    }
}