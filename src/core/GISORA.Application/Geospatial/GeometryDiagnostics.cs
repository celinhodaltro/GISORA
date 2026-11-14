using GISORA.Domain.Geospatial;

namespace GISORA.Application.Geospatial;

public static class GeometryDiagnostics
{
    private const double Tolerance = 1e-9;

    public static IReadOnlyList<GeometryIssue> Inspect(Polygon polygon)
    {
        ArgumentNullException.ThrowIfNull(polygon);

        var issues = new List<GeometryIssue>();

        for (var index = 0; index < polygon.Rings.Count; index++)
        {
            var name = index == 0 ? "Anel externo" : $"Anel interno {index}";
            issues.AddRange(InspectRing(polygon.Rings[index].Coordinates, name));
        }

        return issues;
    }

    public static IReadOnlyList<GeometryIssue> InspectRing(IReadOnlyList<Coordinate> ring, string ringName = "Anel externo")
    {
        ArgumentNullException.ThrowIfNull(ring);

        var issues = new List<GeometryIssue>();

        if (ring.Count == 0)
        {
            issues.Add(new GeometryIssue("geometry.ring.minimum", $"{ringName} nao possui coordenadas."));

            return issues;
        }

        if (ring.Count < 4)
        {
            issues.Add(new GeometryIssue(
                "geometry.ring.minimum",
                $"{ringName} exige ao menos 4 coordenadas. Atual: {ring.Count}.",
                ring[0]));
        }

        if (ring.Count > 1 && ring[0].Equals(ring[^1]) is false)
        {
            issues.Add(new GeometryIssue(
                "geometry.ring.open",
                $"{ringName} deve estar fechado (primeira e ultima coordenadas iguais).",
                ring[0]));
        }

        AddDuplicatedVertices(ring, ringName, issues);
        AddSelfIntersections(ring, ringName, issues);

        return issues;
    }

    private static void AddDuplicatedVertices(IReadOnlyList<Coordinate> ring, string ringName, List<GeometryIssue> issues)
    {
        var lastIndex = ring.Count > 1 && ring[0].Equals(ring[^1]) ? ring.Count - 1 : ring.Count;
        var unique = new List<Coordinate>();

        for (var index = 0; index < lastIndex; index++)
        {
            var coordinate = ring[index];

            if (unique.Any(existing => existing.Equals(coordinate)))
            {
                issues.Add(new GeometryIssue(
                    "geometry.duplicate-vertex",
                    $"{ringName} possui vertice duplicado na posicao {index + 1}.",
                    coordinate));

                continue;
            }

            unique.Add(coordinate);
        }
    }

    private static void AddSelfIntersections(IReadOnlyList<Coordinate> ring, string ringName, List<GeometryIssue> issues)
    {
        var lastIndex = ring.Count > 1 && ring[0].Equals(ring[^1]) ? ring.Count - 1 : ring.Count;

        if (lastIndex < 4)
        {
            return;
        }

        var vertices = ring.Take(lastIndex).ToArray();
        var segments = vertices
            .Select((coordinate, index) => (Start: coordinate, End: vertices[(index + 1) % vertices.Length], Index: index))
            .Where(segment => segment.Start.Equals(segment.End) is false)
            .ToArray();

        for (var first = 0; first < segments.Length; first++)
        {
            for (var second = first + 1; second < segments.Length; second++)
            {
                if (IsAdjacent(segments[first].Index, segments[second].Index, vertices.Length))
                {
                    continue;
                }

                if (TryIntersect(segments[first].Start, segments[first].End, segments[second].Start, segments[second].End, out var location) is false)
                {
                    continue;
                }

                issues.Add(new GeometryIssue(
                    "geometry.self-intersection",
                    $"{ringName} possui auto-intersecao entre os segmentos {segments[first].Index + 1} e {segments[second].Index + 1}.",
                    location));

                return;
            }
        }
    }

    private static bool IsAdjacent(int first, int second, int segmentCount)
        => second == first + 1 || (first == 0 && second == segmentCount - 1);

    private static bool TryIntersect(Coordinate a, Coordinate b, Coordinate c, Coordinate d, out Coordinate? location)
    {
        location = null;

        var rx = b.X - a.X;
        var ry = b.Y - a.Y;
        var sx = d.X - c.X;
        var sy = d.Y - c.Y;
        var denominator = Cross(rx, ry, sx, sy);
        var qpx = c.X - a.X;
        var qpy = c.Y - a.Y;

        if (Math.Abs(denominator) <= Tolerance)
        {
            if (Math.Abs(Cross(qpx, qpy, rx, ry)) > Tolerance)
            {
                return false;
            }

            var lengthSquared = (rx * rx) + (ry * ry);

            if (lengthSquared <= Tolerance)
            {
                return false;
            }

            var firstParameter = ((qpx * rx) + (qpy * ry)) / lengthSquared;
            var secondParameter = firstParameter + (((sx * rx) + (sy * ry)) / lengthSquared);
            var start = Math.Max(0d, Math.Min(firstParameter, secondParameter));
            var end = Math.Min(1d, Math.Max(firstParameter, secondParameter));

            if (start > end + Tolerance)
            {
                return false;
            }

            location = PointOn(a, rx, ry, Math.Clamp(start, 0d, 1d));

            return true;
        }

        var firstFactor = Cross(qpx, qpy, sx, sy) / denominator;
        var secondFactor = Cross(qpx, qpy, rx, ry) / denominator;

        if (firstFactor < -Tolerance || firstFactor > 1d + Tolerance
            || secondFactor < -Tolerance || secondFactor > 1d + Tolerance)
        {
            return false;
        }

        location = PointOn(a, rx, ry, Math.Clamp(firstFactor, 0d, 1d));

        return true;
    }

    private static Coordinate PointOn(Coordinate origin, double deltaX, double deltaY, double factor)
        => new(origin.X + (deltaX * factor), origin.Y + (deltaY * factor), origin.SpatialReference);

    private static double Cross(double firstX, double firstY, double secondX, double secondY)
        => (firstX * secondY) - (firstY * secondX);
}