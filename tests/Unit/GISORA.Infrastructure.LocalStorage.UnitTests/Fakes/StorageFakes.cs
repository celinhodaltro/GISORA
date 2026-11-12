using System.Text.Json;

using Microsoft.JSInterop;

namespace GISORA.Infrastructure.LocalStorage.UnitTests.Fakes;

internal sealed class InMemoryLocalStore : ILocalStore
{
    private readonly Dictionary<string, string> _items = [];

    public IReadOnlyDictionary<string, string> Items => _items;

    public Task<T?> GetAsync<T>(string storageNamespace, string key, CancellationToken cancellationToken = default)
    {
        var storedKey = LocalStoreKeys.For(storageNamespace, key);

        return Task.FromResult(_items.TryGetValue(storedKey, out var raw)
            ? JsonSerializer.Deserialize<T>(raw)
            : default);
    }

    public Task SetAsync<T>(string storageNamespace, string key, T value, CancellationToken cancellationToken = default)
    {
        _items[LocalStoreKeys.For(storageNamespace, key)] = JsonSerializer.Serialize(value);

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string storageNamespace, string key, CancellationToken cancellationToken = default)
    {
        _items.Remove(LocalStoreKeys.For(storageNamespace, key));

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string storageNamespace, string key, CancellationToken cancellationToken = default)
        => Task.FromResult(_items.ContainsKey(LocalStoreKeys.For(storageNamespace, key)));

    public Task ClearNamespaceAsync(string storageNamespace, CancellationToken cancellationToken = default)
    {
        var prefix = LocalStoreKeys.NamespacePrefix(storageNamespace);

        foreach (var key in _items.Keys.Where(key => key.StartsWith(prefix, StringComparison.Ordinal)).ToArray())
        {
            _items.Remove(key);
        }

        return Task.CompletedTask;
    }

    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);
}

internal sealed class FakeJsStorage : IJsStorage
{
    private readonly Dictionary<string, string> _items = [];

    public JSException? ThrowOnSet { get; set; }

    public JSException? ThrowOnAvailability { get; set; }

    public ValueTask<bool> IsAvailableAsync(CancellationToken cancellationToken)
        => ThrowOnAvailability is not null
            ? throw ThrowOnAvailability
            : ValueTask.FromResult(true);

    public ValueTask<string?> GetItemAsync(string key, CancellationToken cancellationToken)
        => ValueTask.FromResult(_items.TryGetValue(key, out var value) ? value : null);

    public ValueTask SetItemAsync(string key, string value, CancellationToken cancellationToken)
    {
        if (ThrowOnSet is not null)
        {
            throw ThrowOnSet;
        }

        _items[key] = value;

        return ValueTask.CompletedTask;
    }

    public ValueTask RemoveItemAsync(string key, CancellationToken cancellationToken)
    {
        _items.Remove(key);

        return ValueTask.CompletedTask;
    }

    public ValueTask<string[]> GetKeysAsync(string prefix, CancellationToken cancellationToken)
        => ValueTask.FromResult(_items.Keys.Where(key => key.StartsWith(prefix, StringComparison.Ordinal)).ToArray());

    public ValueTask RemoveKeysAsync(string prefix, CancellationToken cancellationToken)
    {
        foreach (var key in _items.Keys.Where(key => key.StartsWith(prefix, StringComparison.Ordinal)).ToArray())
        {
            _items.Remove(key);
        }

        return ValueTask.CompletedTask;
    }
}