using GISORA.Domain.Common;

namespace GISORA.Domain.Geospatial;

public sealed class SpatialReference : ValueObject
{
    private SpatialReference(string authority, int code)
    {
        Authority = authority;
        Code = code;
    }

    public static SpatialReference Wgs84 => FromEpsg(4326);

    public string Authority { get; }

    public int Code { get; }

    public static SpatialReference FromEpsg(int code) => From("EPSG", code);

    public static SpatialReference From(string authority, int code)
    {
        var normalized = Guard.NotNullOrWhiteSpace(authority, nameof(authority)).Trim().ToUpperInvariant();
        Guard.Positive(code, nameof(code));

        return new SpatialReference(normalized, code);
    }

    public override string ToString() => $"{Authority}:{Code}";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Authority;
        yield return Code;
    }
}