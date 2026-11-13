using GISORA.Application.Geospatial;
using GISORA.Domain.Geospatial;

namespace GISORA.Infrastructure.Geospatial;

public sealed class GeoJsonSerializer : IGeoJsonSerializer
{
    public string Serialize(Geometry geometry) => GeoJsonMapper.ToGeoJson(geometry);

    public Geometry Deserialize(string geoJson, SpatialReference reference) => GeoJsonMapper.FromGeoJson(geoJson, reference);
}