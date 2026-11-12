using GISORA.Geospatial.Domain;

namespace GISORA.Geospatial.Application;

public interface IGeoJsonSerializer
{
    string Serialize(Geometry geometry);

    Geometry Deserialize(string geoJson, SpatialReference reference);
}