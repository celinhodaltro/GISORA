using GISORA.Domain.Common;

namespace GISORA.Domain.Geospatial;

public sealed class Coordinate : ValueObject
{
    public Coordinate(double x, double y, SpatialReference spatialReference, double? z = null)
    {
        Guard.NotNull(spatialReference, nameof(spatialReference));
        Guard.Condition(double.IsFinite(x) && double.IsFinite(y), "Coordenada deve ser um numero finito.", "geospatial.coordinate.finite");

        if (z is not null)
        {
            Guard.Condition(double.IsFinite(z.Value), "Coordenada Z deve ser um numero finito.", "geospatial.coordinate.finite");
        }

        X = x;
        Y = y;
        Z = z;
        SpatialReference = spatialReference;
    }

    public double X { get; }

    public double Y { get; }

    public double? Z { get; }

    public SpatialReference SpatialReference { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return X;
        yield return Y;
        yield return Z;
        yield return SpatialReference;
    }
}