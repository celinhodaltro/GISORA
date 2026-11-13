using System.Text.Json.Serialization;

using Microsoft.JSInterop;

namespace GISORA.Infrastructure.Geospatial;

public sealed class GeospatialInterop(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private const string ModulePath = "./_content/GISORA.Infrastructure/js/geospatial.js";

    private readonly SemaphoreSlim _gate = new(1, 1);
    private IJSObjectReference? _module;

    public async ValueTask<InteropValidation> ValidateAsync(string geoJson, CancellationToken cancellationToken)
        => await (await ModuleAsync(cancellationToken))
            .InvokeAsync<InteropValidation>("validate", cancellationToken, geoJson);

    public async ValueTask<string> TransformAsync(string geoJson, int sourceSrid, int targetSrid, CancellationToken cancellationToken)
        => await (await ModuleAsync(cancellationToken))
            .InvokeAsync<string>("transform", cancellationToken, geoJson, sourceSrid, targetSrid);

    public async ValueTask<double> AreaAsync(string geoJson, int srid, CancellationToken cancellationToken)
        => await (await ModuleAsync(cancellationToken))
            .InvokeAsync<double>("area", cancellationToken, geoJson, srid);

    public async ValueTask<double> DistanceAsync(string firstGeoJson, string secondGeoJson, int srid, CancellationToken cancellationToken)
        => await (await ModuleAsync(cancellationToken))
            .InvokeAsync<double>("distance", cancellationToken, firstGeoJson, secondGeoJson, srid);

    public async ValueTask<bool> IntersectsAsync(string firstGeoJson, string secondGeoJson, CancellationToken cancellationToken)
        => await (await ModuleAsync(cancellationToken))
            .InvokeAsync<bool>("intersects", cancellationToken, firstGeoJson, secondGeoJson);

    public async ValueTask<bool> ContainsAsync(string containerGeoJson, string containedGeoJson, CancellationToken cancellationToken)
        => await (await ModuleAsync(cancellationToken))
            .InvokeAsync<bool>("contains", cancellationToken, containerGeoJson, containedGeoJson);

    public async ValueTask<string?> IntersectionAsync(string firstGeoJson, string secondGeoJson, CancellationToken cancellationToken)
        => await (await ModuleAsync(cancellationToken))
            .InvokeAsync<string?>("intersection", cancellationToken, firstGeoJson, secondGeoJson);

    public async ValueTask DisposeAsync()
    {
        if (_module is null)
        {
            return;
        }

        await _module.DisposeAsync();
        _gate.Dispose();
    }

    private async ValueTask<IJSObjectReference> ModuleAsync(CancellationToken cancellationToken)
    {
        if (_module is not null)
        {
            return _module;
        }

        await _gate.WaitAsync(cancellationToken);
        try
        {
            _module ??= await jsRuntime.InvokeAsync<IJSObjectReference>("import", cancellationToken, ModulePath);

            return _module;
        }
        finally
        {
            _gate.Release();
        }
    }
}

public sealed record InteropValidation(
    [property: JsonPropertyName("valid")] bool Valid,
    [property: JsonPropertyName("errors")] string[] Errors);