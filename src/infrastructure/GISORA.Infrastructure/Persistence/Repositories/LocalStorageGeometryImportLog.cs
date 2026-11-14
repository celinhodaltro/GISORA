using GISORA.Application.Properties;
using GISORA.Infrastructure.Persistence.Documents;

namespace GISORA.Infrastructure.Persistence.Repositories;

public sealed class LocalStorageGeometryImportLog(ILocalStore store) : IGeometryImportLog
{
    private const string Key = "geometry-imports";

    public async Task RecordAsync(GeometryImportRecord record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);

        var items = await ReadAsync(cancellationToken);
        items.Add(GeometryImportDocument.FromDomain(record));

        await store.SetAsync(StorageNamespaces.Properties, Key, items, cancellationToken);
    }

    public async Task<IReadOnlyList<GeometryImportRecord>> ListAsync(CancellationToken cancellationToken = default)
    {
        var items = await ReadAsync(cancellationToken);

        return items
            .OrderByDescending(item => item.ImportedAt)
            .Select(item => item.ToDomain())
            .ToArray();
    }

    private async Task<List<GeometryImportDocument>> ReadAsync(CancellationToken cancellationToken)
        => await store.GetAsync<List<GeometryImportDocument>>(StorageNamespaces.Properties, Key, cancellationToken) ?? [];
}