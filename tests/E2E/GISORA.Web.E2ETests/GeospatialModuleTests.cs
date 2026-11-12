namespace GISORA.Web.E2ETests;

[Collection(WebAppGroup.Name)]
public class GeospatialModuleTests(WebAppFixture fixture)
{
    private const string ModulePath = "/_content/GISORA.Infrastructure.Geospatial/js/geospatial.js";

    [Fact]
    public async Task ComputesProjectedAreaInSquareMeters()
    {
        var area = await EvaluateAsync<double>("""
            async () => {
              const module = await import('MODULE');
              const square = JSON.stringify({
                type: 'Polygon',
                coordinates: [[[0, 0], [100, 0], [100, 100], [0, 100], [0, 0]]]
              });
              return module.area(square, 31985);
            }
            """);

        Assert.InRange(area, 9_950, 10_050);
    }

    [Fact]
    public async Task TransformsCoordinatesBetweenCrs()
    {
        var coordinates = await EvaluateAsync<double[]>("""
            async () => {
              const module = await import('MODULE');
              const point = JSON.stringify({ type: 'Point', coordinates: [-35.25, -7.95] });
              const transformed = JSON.parse(module.transform(point, 4326, 31985));
              return transformed.coordinates;
            }
            """);

        Assert.Equal(2, coordinates.Length);
        Assert.True(Math.Abs(coordinates[0]) > 100_000, $"Easting inesperado: {coordinates[0]}");
        Assert.True(Math.Abs(coordinates[1]) > 1_000_000, $"Northing inesperado: {coordinates[1]}");
    }

    [Fact]
    public async Task DetectsSelfIntersectingPolygon()
    {
        var valid = await EvaluateAsync<bool>("""
            async () => {
              const module = await import('MODULE');
              const bowtie = JSON.stringify({
                type: 'Polygon',
                coordinates: [[[0, 0], [100, 100], [100, 0], [0, 100], [0, 0]]]
              });
              return module.validate(bowtie).valid;
            }
            """);

        Assert.False(valid);
    }

    [Fact]
    public async Task AcceptsSimplePolygon()
    {
        var valid = await EvaluateAsync<bool>("""
            async () => {
              const module = await import('MODULE');
              const square = JSON.stringify({
                type: 'Polygon',
                coordinates: [[[0, 0], [100, 0], [100, 100], [0, 100], [0, 0]]]
              });
              return module.validate(square).valid;
            }
            """);

        Assert.True(valid);
    }

    [Fact]
    public async Task DetectsIntersectionAndContainment()
    {
        var result = await EvaluateAsync<bool[]>("""
            async () => {
              const module = await import('MODULE');
              const outer = JSON.stringify({
                type: 'Polygon',
                coordinates: [[[0, 0], [100, 0], [100, 100], [0, 100], [0, 0]]]
              });
              const inner = JSON.stringify({
                type: 'Polygon',
                coordinates: [[[10, 10], [20, 10], [20, 20], [10, 20], [10, 10]]]
              });
              const apart = JSON.stringify({
                type: 'Polygon',
                coordinates: [[[500, 500], [600, 500], [600, 600], [500, 600], [500, 500]]]
              });
              return [
                module.intersects(outer, inner),
                module.intersects(outer, apart),
                module.contains(outer, inner),
                module.contains(inner, outer)
              ];
            }
            """);

        Assert.Equal([true, false, true, false], result);
    }

    private async Task<T> EvaluateAsync<T>(string script)
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync(fixture.BaseUrl);

        var prepared = script.Replace("MODULE", ModulePath, StringComparison.Ordinal);

        return await page.EvaluateAsync<T>(prepared);
    }
}