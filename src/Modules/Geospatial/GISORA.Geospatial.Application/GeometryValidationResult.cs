namespace GISORA.Geospatial.Application;

/// <summary>
/// Resultado de validacao geometrica. Erros em texto descritivo, prontos para a UI.
/// </summary>
public sealed record GeometryValidationResult(bool IsValid, IReadOnlyList<string> Errors)
{
    public static GeometryValidationResult Valid { get; } = new(true, []);

    public static GeometryValidationResult Invalid(params string[] errors) => new(false, errors);
}