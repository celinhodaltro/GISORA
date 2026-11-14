using GISORA.Application.Common;
using GISORA.Application.Geospatial;
using GISORA.Application.Properties;
using GISORA.Application.Properties.Commands;
using GISORA.Domain.Geospatial;
using GISORA.Domain.Properties;
using GISORA.Infrastructure.Geospatial;
using GISORA.UnitTests.Properties.Fakes;

namespace GISORA.UnitTests.Properties;

public class PropertyGeoJsonTests
{
    [Fact]
    public async Task ExportsAndReimportsPropertyGeoJson()
    {
        var property = PropertyFactory.Create();
        property.ChangeGeometry(PropertyFactory.Square(0, 0, 10), null);

        var repository = new InMemoryPropertyRepository();
        await repository.AddAsync(property);

        var view = new PropertyGeometryView(
            property.Id,
            property.Registration.Formatted,
            property.Geometry,
            property.LandArea.SquareMeters);
        var transformer = new StubCoordinateTransformer();
        var serializer = new GeoJsonSerializer();
        var export = new PropertyGeoJsonExportHandler(
            new StubPropertyQueries(view),
            new InMemoryPropertyMapQueries(),
            new GeoJsonExportService(transformer, serializer));

        var exported = await export.ExportPropertyAsync(property.Id, "EPSG:4326");

        Assert.True(exported.IsSuccess);
        Assert.EndsWith(".geojson", exported.Value.FileName, StringComparison.Ordinal);
        Assert.Contains("\"FeatureCollection\"", exported.Value.Content, StringComparison.Ordinal);
        Assert.Contains("001.002.003", exported.Value.Content, StringComparison.Ordinal);
        Assert.Contains("\"sourceCrs\":\"EPSG:4326\"", exported.Value.Content, StringComparison.Ordinal);
        Assert.Equal(0, transformer.Calls);

        var log = new StubImportLog();
        var import = new ImportPropertyGeometryHandler(
            serializer,
            transformer,
            repository,
            new StubPropertyQueries(view),
            new FakePropertyGeometryValidator(),
            log);

        var imported = await import.HandleAsync(new ImportPropertyGeometryCommand(
            "imovel.geojson",
            exported.Value.Content));

        Assert.True(imported.IsSuccess);
        Assert.Single(imported.Value.Updated);
        Assert.Empty(imported.Value.Skipped);
        Assert.Single(log.Records);
        Assert.Equal("EPSG:4326", log.Records[0].SourceCrs);
        Assert.Equal("imovel.geojson", log.Records[0].Origin);
    }

    [Fact]
    public async Task UsesTransformerWhenExportingToProjectedCrs()
    {
        var property = PropertyFactory.Create();
        property.ChangeGeometry(PropertyFactory.Square(0, 0, 10), null);

        var view = new PropertyGeometryView(
            property.Id,
            property.Registration.Formatted,
            property.Geometry,
            property.LandArea.SquareMeters);
        var target = SpatialReference.FromEpsg(31985);
        var transformer = new StubCoordinateTransformer((geometry, reference) => ProjectedSquare(reference));
        var serializer = new GeoJsonSerializer();
        var export = new PropertyGeoJsonExportHandler(
            new StubPropertyQueries(view),
            new InMemoryPropertyMapQueries(),
            new GeoJsonExportService(transformer, serializer));

        var exported = await export.ExportPropertyAsync(property.Id, "EPSG:31985");

        Assert.True(exported.IsSuccess);
        Assert.Equal(1, transformer.Calls);
        Assert.Equal(target, transformer.LastTarget);
        Assert.Contains("\"targetCrs\":\"EPSG:31985\"", exported.Value.Content, StringComparison.Ordinal);

        var parsed = serializer.DeserializeFeatures(exported.Value.Content, target);
        var polygon = Assert.IsType<Polygon>(Assert.Single(parsed.Features).Geometry);
        Assert.Equal(300_000, polygon.ExteriorRing.Coordinates[0].X);
        Assert.Equal(target, polygon.SpatialReference);
    }

    [Fact]
    public async Task RejectsInvalidCrsOnExportAndImport()
    {
        var property = PropertyFactory.Create();
        property.ChangeGeometry(PropertyFactory.Square(0, 0, 10), null);

        var view = new PropertyGeometryView(
            property.Id,
            property.Registration.Formatted,
            property.Geometry,
            property.LandArea.SquareMeters);
        var serializer = new GeoJsonSerializer();
        var export = new PropertyGeoJsonExportHandler(
            new StubPropertyQueries(view),
            new InMemoryPropertyMapQueries(),
            new GeoJsonExportService(new StubCoordinateTransformer(), serializer));

        var exported = await export.ExportPropertyAsync(property.Id, "EPSG:9999");

        Assert.True(exported.IsFailure);
        Assert.Equal("geospatial.crs.invalid", exported.Error.Code);
        Assert.Contains("EPSG:9999", exported.Error.Message, StringComparison.Ordinal);

        var import = new ImportPropertyGeometryHandler(
            serializer,
            new StubCoordinateTransformer(),
            new InMemoryPropertyRepository(),
            new StubPropertyQueries(view),
            new FakePropertyGeometryValidator(),
            new StubImportLog());

        var imported = await import.HandleAsync(new ImportPropertyGeometryCommand(
            "imovel.geojson",
            "{\"type\":\"Point\",\"coordinates\":[0,0]}",
            "EPSG:1234",
            "EPSG:4326"));

        Assert.True(imported.IsFailure);
        Assert.Equal("geospatial.crs.invalid", imported.Error.Code);
    }

    [Fact]
    public async Task RejectsImportWithoutMatchingProperty()
    {
        var serializer = new GeoJsonSerializer();
        var import = new ImportPropertyGeometryHandler(
            serializer,
            new StubCoordinateTransformer(),
            new InMemoryPropertyRepository(),
            new StubPropertyQueries(null),
            new FakePropertyGeometryValidator(),
            new StubImportLog());

        var imported = await import.HandleAsync(new ImportPropertyGeometryCommand(
            "imovel.geojson",
            "{\"type\":\"FeatureCollection\",\"features\":[{\"type\":\"Feature\",\"id\":\"00000000-0000-0000-0000-000000000001\",\"properties\":{\"inscricao\":\"999.999.999\"},\"geometry\":{\"type\":\"Polygon\",\"coordinates\":[[[0,0],[10,0],[10,10],[0,10],[0,0]]]}}]}"));

        Assert.True(imported.IsFailure);
        Assert.Equal("properties.geometry-import.empty", imported.Error.Code);
        Assert.Contains("sem imovel correspondente", imported.Error.Message, StringComparison.Ordinal);
    }

    private static Polygon ProjectedSquare(SpatialReference reference)
        => new([
            new CoordinateSequence([
                new Coordinate(300_000, 9_120_000, reference),
                new Coordinate(300_010, 9_120_000, reference),
                new Coordinate(300_010, 9_120_010, reference),
                new Coordinate(300_000, 9_120_010, reference),
                new Coordinate(300_000, 9_120_000, reference),
            ]),
        ]);
}

internal sealed class StubPropertyQueries(
    PropertyGeometryView? view,
    IReadOnlyList<PropertyListItem>? items = null) : IPropertyQueries
{
    public Task<PagedResult<PropertyListItem>> SearchAsync(PropertyFilter filter, CancellationToken cancellationToken = default)
        => Task.FromResult(new PagedResult<PropertyListItem>(items ?? [], 1, filter.PageSize, items?.Count ?? 0));

    public Task<PropertyGeometryView?> GetGeometryAsync(PropertyId id, CancellationToken cancellationToken = default)
        => Task.FromResult(view is not null && view.Id == id ? view : null);
}

internal sealed class StubCoordinateTransformer(
    Func<Geometry, SpatialReference, Geometry>? map = null) : ICoordinateTransformer
{
    public int Calls { get; private set; }

    public SpatialReference? LastTarget { get; private set; }

    public Task<Geometry> TransformAsync(
        Geometry geometry,
        SpatialReference target,
        CancellationToken cancellationToken = default)
    {
        Calls++;
        LastTarget = target;

        if (map is null)
        {
            throw new InvalidOperationException("Transformacao nao esperada neste teste.");
        }

        return Task.FromResult(map(geometry, target));
    }
}

internal sealed class StubImportLog : IGeometryImportLog
{
    public List<GeometryImportRecord> Records { get; } = [];

    public Task RecordAsync(GeometryImportRecord record, CancellationToken cancellationToken = default)
    {
        Records.Add(record);

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<GeometryImportRecord>> ListAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<GeometryImportRecord>>(Records);
}