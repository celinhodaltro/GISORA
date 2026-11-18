using GISORA.Application.Identity;

using Microsoft.JSInterop;

namespace GISORA.Infrastructure.Identity;

public sealed class CookieSessionStore : ISessionStore, IAsyncDisposable
{
    internal const string CookieName = "gisora_session";

    private const string ModulePath = "./_content/GISORA.Infrastructure/js/session.js";

    private readonly IJSRuntime _jsRuntime;
    private readonly SemaphoreSlim _gate = new(1, 1);

    private IJSObjectReference? _module;

    public CookieSessionStore(IJSRuntime jsRuntime) => _jsRuntime = jsRuntime;

    public async Task<string?> ReadAsync(CancellationToken cancellationToken = default)
        => await (await ModuleAsync(cancellationToken)).InvokeAsync<string?>("read", cancellationToken, CookieName);

    public async Task WriteAsync(AccessToken token, CancellationToken cancellationToken = default)
    {
        var maxAgeSeconds = Math.Max(0, (int)(token.ExpiresAt - DateTimeOffset.UtcNow).TotalSeconds);

        await (await ModuleAsync(cancellationToken)).InvokeVoidAsync(
            "write",
            cancellationToken,
            CookieName,
            token.Value,
            maxAgeSeconds);
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
        => await (await ModuleAsync(cancellationToken)).InvokeVoidAsync("clear", cancellationToken, CookieName);

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
            _module ??= await _jsRuntime.InvokeAsync<IJSObjectReference>("import", cancellationToken, ModulePath);

            return _module;
        }
        finally
        {
            _gate.Release();
        }
    }
}