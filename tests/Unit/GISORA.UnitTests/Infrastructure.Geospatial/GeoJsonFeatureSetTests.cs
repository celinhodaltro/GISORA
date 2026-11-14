using GISORA.Application.Geospatial;
using GISORA.Domain.Geospatial;
using GISORA.Infrastructure.Geospatial;
using GISORA.UnitTests.Properties;

namespace GISORA.UnitTests.Infrastructure.Geospatial;

public class GeoJsonFeatureSetTests
{
    private static readonly SpatialReference Wgs84 = SpatialReference.Wgs84;

    [Fact]
    public void RoundTripsFeatureSetWithMetadataAndProperties()
    {
        var serializer = new GeoJsonSerializer();
        var collection = new GeoJsonFeatureSet(
            [
                new GeoJsonFeature(
                    "imovel-1",
                    PropertyFactory.Square(-35.25, -7.95, 0.01),
                    new Dictionary<string, string?>(StringComparer.Ordinal)
                    {
                        ["inscricao"] = "001.002.003",
                        ["tipo"] = "imovel",
                    }),
            ],
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                ["sourceCrs"] = "EPSG:4326",
                ["targetCrs"] = "EPSG:4326",
                ["origin"] = "imovel",
            });

        var json = serializer.SerializeFeatures(collection);
        var parsed = serializer.DeserializeFeatures(json, Wgs84);

        var feature = Assert.Single(parsed.Features);
        Assert.Equal("imovel-1", feature.Id);
        Assert.Equal("001.002.003", feature.Properties["inscricao"]);
        Assert.Equal("EPSG:4326", parsed.Metadata!["sourceCrs"]);
        Assert.IsType<Polygon>(feature.Geometry);
        Assert.Contains("\"type\":\"FeatureCollection\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public void ReadsLegacyCrsMemberAsSourceMetadata()
    {
        const string json = """
            {
              "type": "FeatureCollection",
              "crs": { "type": "name", "properties": { "name": "EPSG:31985" } },
              "features": [
                {
                  "type": "Feature",
                  "properties": { "inscricao": "001.002.003" },
                  "geometry": { "type": "Point", "coordinates": [300000, 9120000] }
                }
              ]
            }
            """;

        var parsed = new GeoJsonSerializer().DeserializeFeatures(json, SpatialReference.FromEpsg(31985));

        Assert.Equal("EPSG:31985", parsed.Metadata!["sourceCrs"]);
        Assert.IsType<Point>(Assert.Single(parsed.Features).Geometry);
    }

    [Fact]
    public void AcceptsBareGeometryAsSingleFeature()
    {
        var parsed = new GeoJsonSerializer().DeserializeFeatures(
            "{\"type\":\"Point\",\"coordinates\":[-35.25,-7.95]}",
            Wgs84);

        var feature = Assert.Single(parsed.Features);
        Assert.IsType<Point>(feature.Geometry);
        Assert.Empty(feature.Properties);
    }

    [Fact]
    public void ReportsInvalidPayloadWithoutThrowing()
    {
        var result = new GeoJsonSerializer().TryDeserializeFeatures("not geojson", Wgs84);

        Assert.False(result.Success);
        Assert.Null(result.Collection);
        Assert.False(string.IsNullOrWhiteSpace(result.Error));
    }

    [Fact]
    public void RejectsNonNumericCoordinate()
    {
        var result = new GeoJsonSerializer().TryDeserializeFeatures(
            "{\"type\":\"Point\",\"coordinates\":[\"x\",\"y\"]}",
            Wgs84);

        Assert.False(result.Success);
        Assert.False(string.IsNullOrWhiteSpace(result.Error));
    }

    [Fact]
    public void RejectsFeatureWithoutGeometry()
    {
        var result = new GeoJsonSerializer().TryDeserializeFeatures(
            "{\"type\":\"FeatureCollection\",\"features\":[{\"type\":\"Feature\",\"properties\":{}}]}",
            Wgs84);

        Assert.False(result.Success);
        Assert.Contains("sem geometria", result.Error, StringComparison.Ordinal);
    }
}