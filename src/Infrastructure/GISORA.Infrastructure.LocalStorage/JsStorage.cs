using Microsoft.JSInterop;

namespace GISORA.Infrastructure.LocalStorage;

internal interface IJsStorage
{
    ValueTask<bool> IsAvailableAsync(CancellationToken cancellationToken);

    ValueTask<string?> GetItemAsync(string key, CancellationToken cancellationToken);

    ValueTask SetItemAsync(string key, string value, CancellationToken cancellationToken);

    ValueTask RemoveItemAsync(string key, CancellationToken cancellationToken);

    ValueTask<string[]> GetKeysAsync(string prefix, CancellationToken cancellationToken);

    ValueTask RemoveKeysAsync(string prefix, CancellationToken cancellationToken);
}

internal sealed class JsStorage : IJsStorage, IAsyncDisposable
{
    private const string ModulePath = "./_content/GISORA.Infrastructure.LocalStorage/js/localstore.js";

    private readonly IJSRuntime _jsRuntime;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private IJSObjectReference? _module;

    public JsStorage(IJSRuntime jsRuntime) => _jsRuntime = jsRuntime;

    public async ValueTask<bool> IsAvailableAsync(CancellationToken cancellationToken)
        => await (await ModuleAsync(cancellationToken)).InvokeAsync<bool>("isAvailable", cancellationToken);

    public async ValueTask<string?> GetItemAsync(string key, CancellationToken cancellationToken)
        => await (await ModuleAsync(cancellationToken)).InvokeAsync<string?>("getItem", cancellationToken, key);

    public async ValueTask SetItemAsync(string key, string value, CancellationToken cancellationToken)
        => await (await ModuleAsync(cancellationToken)).InvokeVoidAsync("setItem", cancellationToken, key, value);

    public async ValueTask RemoveItemAsync(string key, CancellationToken cancellationToken)
        => await (await ModuleAsync(cancellationToken)).InvokeVoidAsync("removeItem", cancellationToken, key);

    public async ValueTask<string[]> GetKeysAsync(string prefix, CancellationToken cancellationToken)
        => await (await ModuleAsync(cancellationToken)).InvokeAsync<string[]>("getKeys", cancellationToken, prefix);

    public async ValueTask RemoveKeysAsync(string prefix, CancellationToken cancellationToken)
        => await (await ModuleAsync(cancellationToken)).InvokeVoidAsync("removeKeys", cancellationToken, prefix);

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