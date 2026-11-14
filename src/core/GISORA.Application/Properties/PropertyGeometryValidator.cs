using System.Globalization;

using GISORA.Application.Geospatial;
using GISORA.Domain.Geospatial;
using GISORA.Domain.Properties;

namespace GISORA.Application.Properties;

public sealed class PropertyGeometryValidator(
    IGeometryValidator geometryValidator,
    IPropertyMapQueries mapQueries,
    ISpatialOperations spatial) : IPropertyGeometryValidator
{
    public const string OverlapCode = "properties.geometry.overlap";

    private const decimal OverlapToleranceSquareMeters = 1m;

    public async Task<GeometryValidationResult> ValidateAsync(
        Polygon geometry,
        PropertyId? excludedPropertyId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(geometry);

        var issues = new List<GeometryIssue>(GeometryDiagnostics.Inspect(geometry));
        var structural = await geometryValidator.ValidateAsync(geometry, cancellationToken);

        foreach (var issue in structural.Issues)
        {
            if (issues.Any(existing => existing.Code == issue.Code))
            {
                continue;
            }

            issues.Add(issue);
        }

        if (issues.Count > 0)
        {
            return GeometryValidationResult.FromIssues(issues);
        }

        var candidates = await mapQueries.GetMapItemsAsync(cancellationToken);

        foreach (var candidate in candidates)
        {
            if (excludedPropertyId is { } excluded && candidate.Id == excluded)
            {
                continue;
            }

            if (candidate.Geometry.SpatialReference != geometry.SpatialReference)
            {
                continue;
            }

            if (candidate.Geometry.GetBoundingBox().Intersects(geometry.GetBoundingBox()) is false)
            {
                continue;
            }

            var intersection = await spatial.IntersectionAsync(geometry, candidate.Geometry, cancellationToken);

            if (intersection is null)
            {
                continue;
            }

            var overlapArea = (await spatial.AreaAsync(intersection, cancellationToken)).SquareMeters;

            if (overlapArea <= OverlapToleranceSquareMeters)
            {
                continue;
            }

            issues.Add(new GeometryIssue(
                OverlapCode,
                $"Geometria sobrepoe o imovel {candidate.Registration} em {overlapArea.ToString("N2", CultureInfo.InvariantCulture)} m2.",
                intersection.Coordinates.Count > 0 ? intersection.Coordinates[0] : null));
        }

        return GeometryValidationResult.FromIssues(issues);
    }
}