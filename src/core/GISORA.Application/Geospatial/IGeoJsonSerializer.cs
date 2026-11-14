using GISORA.Domain.Geospatial;

namespace GISORA.Application.Geospatial;

public interface IGeoJsonSerializer
{
    string Serialize(Geometry geometry);

    Geometry Deserialize(string geoJson, SpatialReference reference);

    string SerializeFeatures(GeoJsonFeatureSet collection);

    GeoJsonFeatureSet DeserializeFeatures(string geoJson, SpatialReference reference);

    GeoJsonParseResult TryDeserializeFeatures(string geoJson, SpatialReference reference);
}