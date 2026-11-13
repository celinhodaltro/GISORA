namespace GISORA.Application.Geospatial;

public sealed record GeometryValidationResult(bool IsValid, IReadOnlyList<string> Errors)
{
    public static GeometryValidationResult Valid { get; } = new(true, []);

    public static GeometryValidationResult Invalid(params string[] errors) => new(false, errors);
}