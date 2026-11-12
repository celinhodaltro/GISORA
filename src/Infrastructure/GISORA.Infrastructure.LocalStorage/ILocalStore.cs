namespace GISORA.Infrastructure.LocalStorage;

public interface ILocalStore
{
    Task<T?> GetAsync<T>(string storageNamespace, string key, CancellationToken cancellationToken = default);

    Task SetAsync<T>(string storageNamespace, string key, T value, CancellationToken cancellationToken = default);

    Task RemoveAsync(string storageNamespace, string key, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string storageNamespace, string key, CancellationToken cancellationToken = default);

    Task ClearNamespaceAsync(string storageNamespace, CancellationToken cancellationToken = default);

    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
}