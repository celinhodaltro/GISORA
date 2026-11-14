namespace GISORA.Application.Geospatial;

public sealed record GeometryValidationResult(bool IsValid, IReadOnlyList<GeometryIssue> Issues)
{
    public IReadOnlyList<string> Errors => Issues.Select(issue => issue.Message).ToArray();

    public static GeometryValidationResult Valid { get; } = new(true, []);

    public static GeometryValidationResult Invalid(params GeometryIssue[] issues) => new(false, issues);

    public static GeometryValidationResult Invalid(params string[] errors)
        => new(false, errors.Select(message => new GeometryIssue("geometry.invalid", message)).ToArray());

    public static GeometryValidationResult FromIssues(IEnumerable<GeometryIssue> issues)
    {
        ArgumentNullException.ThrowIfNull(issues);

        var items = issues.ToArray();

        return new GeometryValidationResult(items.Length == 0, items);
    }
}