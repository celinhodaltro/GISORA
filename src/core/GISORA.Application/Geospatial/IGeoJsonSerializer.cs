using GISORA.Domain.Geospatial;

namespace GISORA.Application.Geospatial;

public interface IGeoJsonSerializer
{
    string Serialize(Geometry geometry);

    Geometry Deserialize(string geoJson, SpatialReference reference);
}