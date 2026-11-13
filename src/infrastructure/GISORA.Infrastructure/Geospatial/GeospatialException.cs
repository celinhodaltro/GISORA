namespace GISORA.Infrastructure.Geospatial;

public sealed class GeospatialException : Exception
{
    public GeospatialException(string message)
        : base(message)
    {
    }

    public GeospatialException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}