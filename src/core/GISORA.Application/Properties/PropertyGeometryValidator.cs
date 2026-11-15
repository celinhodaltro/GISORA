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
        Geometry geometry,
        PropertyId? excludedPropertyId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(geometry);

        var polygons = geometry switch
        {
            Polygon polygon => [polygon],
            MultiPolygon multiPolygon => multiPolygon.Polygons,
            _ => [],
        };

        if (polygons.Count == 0)
        {
            return GeometryValidationResult.FromIssues(
                [new GeometryIssue("properties.geometry.type", "Geometria deve ser poligono ou multipoligono.", null)]);
        }

        var issues = new List<GeometryIssue>();

        foreach (var polygon in polygons)
        {
            foreach (var issue in GeometryDiagnostics.Inspect(polygon))
            {
                if (issues.Any(existing => existing.Code == issue.Code))
                {
                    continue;
                }

                issues.Add(issue);
            }

            var structural = await geometryValidator.ValidateAsync(polygon, cancellationToken);

            foreach (var issue in structural.Issues)
            {
                if (issues.Any(existing => existing.Code == issue.Code))
                {
                    continue;
                }

                issues.Add(issue);
            }
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

            foreach (var polygon in polygons)
            {
                var intersection = await spatial.IntersectionAsync(polygon, candidate.Geometry, cancellationToken);

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
        }

        return GeometryValidationResult.FromIssues(issues);
    }
}