using System.Text.Json;

using Microsoft.JSInterop;

namespace GISORA.Infrastructure.LocalStorage;

public sealed class BrowserLocalStore : ILocalStore
{
    private readonly IJsStorage _storage;

    public BrowserLocalStore(IJSRuntime jsRuntime)
        : this(new JsStorage(jsRuntime))
    {
    }

    internal BrowserLocalStore(IJsStorage storage) => _storage = storage;

    public async Task<T?> GetAsync<T>(string storageNamespace, string key, CancellationToken cancellationToken = default)
    {
        var storedKey = LocalStoreKeys.For(storageNamespace, key);
        var raw = await ExecuteAsync(
            token => _storage.GetItemAsync(storedKey, token),
            cancellationToken);

        if (raw is null)
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(raw, LocalStorageJson.Options);
        }
        catch (JsonException exception)
        {
            throw new StorageException($"Conteudo invalido em '{storedKey}'.", exception);
        }
    }

    public async Task SetAsync<T>(string storageNamespace, string key, T value, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(value);

        var storedKey = LocalStoreKeys.For(storageNamespace, key);
        var json = JsonSerializer.Serialize(value, LocalStorageJson.Options);

        await ExecuteAsync(
            async token =>
            {
                await _storage.SetItemAsync(storedKey, json, token);

                return true;
            },
            cancellationToken);
    }

    public async Task RemoveAsync(string storageNamespace, string key, CancellationToken cancellationToken = default)
    {
        var storedKey = LocalStoreKeys.For(storageNamespace, key);

        await ExecuteAsync(
            async token =>
            {
                await _storage.RemoveItemAsync(storedKey, token);

                return true;
            },
            cancellationToken);
    }

    public Task<bool> ExistsAsync(string storageNamespace, string key, CancellationToken cancellationToken = default)
    {
        var storedKey = LocalStoreKeys.For(storageNamespace, key);

        return ExecuteAsync(
            async token => await _storage.GetItemAsync(storedKey, token) is not null,
            cancellationToken);
    }

    public async Task ClearNamespaceAsync(string storageNamespace, CancellationToken cancellationToken = default)
    {
        var prefix = LocalStoreKeys.NamespacePrefix(storageNamespace);

        await ExecuteAsync(
            async token =>
            {
                await _storage.RemoveKeysAsync(prefix, token);

                return true;
            },
            cancellationToken);
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _storage.IsAvailableAsync(cancellationToken);
        }
        catch (JSException)
        {
            return false;
        }
    }

    private static async Task<T> ExecuteAsync<T>(Func<CancellationToken, ValueTask<T>> action, CancellationToken cancellationToken)
    {
        try
        {
            return await action(cancellationToken);
        }
        catch (JSException exception) when (IsQuotaExceeded(exception))
        {
            throw new StorageQuotaExceededException(exception);
        }
        catch (JSException exception)
        {
            throw new StorageException("Falha ao acessar o armazenamento local.", exception);
        }
    }

    private static bool IsQuotaExceeded(JSException exception)
        => exception.Message.Contains("QuotaExceeded", StringComparison.OrdinalIgnoreCase)
           || exception.Message.Contains("quota", StringComparison.OrdinalIgnoreCase);
}