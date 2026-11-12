using GISORA.Geospatial.Application;
using GISORA.Geospatial.Domain;

namespace GISORA.Infrastructure.Geospatial;

internal static class GeospatialCall
{
    public static async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
    {
        try
        {
            return await action();
        }
        catch (Microsoft.JSInterop.JSException exception)
        {
            throw new GeospatialException("Falha ao executar operacao geografica no navegador.", exception);
        }
    }
}

public sealed class TurfGeometryValidator(GeospatialInterop interop) : IGeometryValidator
{
    public async Task<GeometryValidationResult> ValidateAsync(Geometry geometry, CancellationToken cancellationToken = default)
    {
        var result = await GeospatialCall.ExecuteAsync(
            () => interop.ValidateAsync(GeoJsonMapper.ToGeoJson(geometry), cancellationToken).AsTask());

        return result.Valid
            ? GeometryValidationResult.Valid
            : GeometryValidationResult.Invalid(result.Errors);
    }
}

public sealed class Proj4CoordinateTransformer(GeospatialInterop interop) : ICoordinateTransformer
{
    public async Task<Geometry> TransformAsync(Geometry geometry, SpatialReference target, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(geometry);
        ArgumentNullException.ThrowIfNull(target);

        var transformed = await GeospatialCall.ExecuteAsync(
            () => interop.TransformAsync(
                GeoJsonMapper.ToGeoJson(geometry),
                geometry.SpatialReference.Code,
                target.Code,
                cancellationToken).AsTask());

        return GeoJsonMapper.FromGeoJson(transformed, target);
    }
}

public sealed class TurfSpatialOperations(GeospatialInterop interop) : ISpatialOperations
{
    public async Task<Area> AreaAsync(Geometry geometry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(geometry);

        var squareMeters = await GeospatialCall.ExecuteAsync(
            () => interop.AreaAsync(GeoJsonMapper.ToGeoJson(geometry), geometry.SpatialReference.Code, cancellationToken).AsTask());

        return Area.FromSquareMeters((decimal)Math.Max(squareMeters, 0));
    }

    public async Task<Distance> DistanceAsync(Geometry origin, Geometry target, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(origin);
        ArgumentNullException.ThrowIfNull(target);

        var meters = await GeospatialCall.ExecuteAsync(
            () => interop.DistanceAsync(
                GeoJsonMapper.ToGeoJson(origin),
                GeoJsonMapper.ToGeoJson(target),
                origin.SpatialReference.Code,
                cancellationToken).AsTask());

        return Distance.FromMeters((decimal)Math.Max(meters, 0));
    }

    public async Task<bool> IntersectsAsync(Geometry left, Geometry right, CancellationToken cancellationToken = default)
        => await GeospatialCall.ExecuteAsync(
            () => interop.IntersectsAsync(
                GeoJsonMapper.ToGeoJson(left),
                GeoJsonMapper.ToGeoJson(right),
                cancellationToken).AsTask());

    public async Task<bool> ContainsAsync(Geometry container, Geometry contained, CancellationToken cancellationToken = default)
        => await GeospatialCall.ExecuteAsync(
            () => interop.ContainsAsync(
                GeoJsonMapper.ToGeoJson(container),
                GeoJsonMapper.ToGeoJson(contained),
                cancellationToken).AsTask());

    public async Task<Geometry?> IntersectionAsync(Geometry left, Geometry right, CancellationToken cancellationToken = default)
    {
        var result = await GeospatialCall.ExecuteAsync(
            () => interop.IntersectionAsync(
                GeoJsonMapper.ToGeoJson(left),
                GeoJsonMapper.ToGeoJson(right),
                cancellationToken).AsTask());

        return result is null ? null : GeoJsonMapper.FromGeoJson(result, left.SpatialReference);
    }
}