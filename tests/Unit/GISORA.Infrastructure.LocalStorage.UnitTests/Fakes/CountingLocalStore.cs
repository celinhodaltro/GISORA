namespace GISORA.Infrastructure.LocalStorage.UnitTests.Fakes;

internal sealed class CountingLocalStore(ILocalStore inner) : ILocalStore
{
    public int GetCount { get; private set; }

    public int SetCount { get; private set; }

    public void ResetCounters()
    {
        GetCount = 0;
        SetCount = 0;
    }

    public Task<T?> GetAsync<T>(string storageNamespace, string key, CancellationToken cancellationToken = default)
    {
        GetCount++;

        return inner.GetAsync<T>(storageNamespace, key, cancellationToken);
    }

    public Task SetAsync<T>(string storageNamespace, string key, T value, CancellationToken cancellationToken = default)
    {
        SetCount++;

        return inner.SetAsync(storageNamespace, key, value, cancellationToken);
    }

    public Task RemoveAsync(string storageNamespace, string key, CancellationToken cancellationToken = default)
        => inner.RemoveAsync(storageNamespace, key, cancellationToken);

    public Task<bool> ExistsAsync(string storageNamespace, string key, CancellationToken cancellationToken = default)
    {
        GetCount++;

        return inner.ExistsAsync(storageNamespace, key, cancellationToken);
    }

    public Task ClearNamespaceAsync(string storageNamespace, CancellationToken cancellationToken = default)
        => inner.ClearNamespaceAsync(storageNamespace, cancellationToken);

    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
        => inner.IsAvailableAsync(cancellationToken);
}