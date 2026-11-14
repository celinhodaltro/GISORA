using GISORA.Application.Geospatial;
using GISORA.Domain.Common;
using GISORA.Domain.Geospatial;

namespace GISORA.Infrastructure.Geospatial;

public sealed class GeoJsonSerializer : IGeoJsonSerializer
{
    public string Serialize(Geometry geometry) => GeoJsonMapper.ToGeoJson(geometry);

    public Geometry Deserialize(string geoJson, SpatialReference reference) => GeoJsonMapper.FromGeoJson(geoJson, reference);

    public string SerializeFeatures(GeoJsonFeatureSet collection) => GeoJsonMapper.ToFeatureCollection(collection);

    public GeoJsonFeatureSet DeserializeFeatures(string geoJson, SpatialReference reference)
        => GeoJsonMapper.FromFeatures(geoJson, reference);

    public GeoJsonParseResult TryDeserializeFeatures(string geoJson, SpatialReference reference)
    {
        try
        {
            return GeoJsonParseResult.Ok(GeoJsonMapper.FromFeatures(geoJson, reference));
        }
        catch (GeospatialException exception)
        {
            return GeoJsonParseResult.Fail(exception.Message);
        }
        catch (DomainException exception)
        {
            return GeoJsonParseResult.Fail($"GeoJSON invalido: {exception.Message}");
        }
        catch (System.Text.Json.JsonException exception)
        {
            return GeoJsonParseResult.Fail($"GeoJSON invalido: {exception.Message}");
        }
        catch (InvalidOperationException exception)
        {
            return GeoJsonParseResult.Fail($"GeoJSON invalido: {exception.Message}");
        }
        catch (FormatException exception)
        {
            return GeoJsonParseResult.Fail($"GeoJSON invalido: {exception.Message}");
        }
        catch (InvalidCastException exception)
        {
            return GeoJsonParseResult.Fail($"GeoJSON invalido: {exception.Message}");
        }
    }
}