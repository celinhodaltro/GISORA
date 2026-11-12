using GISORA.Infrastructure.LocalStorage.Persistence;
using GISORA.Properties.Domain;

namespace GISORA.Infrastructure.LocalStorage.Repositories;

public sealed class LocalStoragePropertyRepository(ILocalStore store) : IPropertyRepository
{
    private const string IndexKey = "property-index";
    private const string IdsKey = "property-ids";

    public async Task<Property?> FindAsync(PropertyId id, CancellationToken cancellationToken = default)
    {
        var document = await store.GetAsync<PropertyDocument>(StorageNamespaces.Properties, Key(id), cancellationToken);

        return document?.ToDomain();
    }

    public async Task<bool> ExistsByRegistrationAsync(PropertyRegistration registration, CancellationToken cancellationToken = default)
    {
        var index = await ReadIndexAsync(cancellationToken);
        var normalized = registration.Formatted.ToUpperInvariant();

        return index.Any(entry => entry.RegistrationSearch == normalized);
    }

    public async Task AddAsync(Property property, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(property);

        var index = await ReadIndexAsync(cancellationToken);
        var normalized = property.Registration.Formatted.ToUpperInvariant();

        if (index.Any(entry => entry.RegistrationSearch == normalized))
        {
            throw new StorageException("Ja existe imovel com esta inscricao no armazenamento local.");
        }

        await store.SetAsync(StorageNamespaces.Properties, Key(property.Id), PropertyDocument.FromDomain(property), cancellationToken);

        var ids = await ReadIdsAsync(cancellationToken);
        if (ids.Contains(property.Id.Value) is false)
        {
            ids.Add(property.Id.Value);
            await store.SetAsync(StorageNamespaces.Properties, IdsKey, ids, cancellationToken);
        }

        index.Add(PropertyIndexEntry.FromDomain(property));
        await store.SetAsync(StorageNamespaces.Properties, IndexKey, index, cancellationToken);
    }

    public async Task UpdateAsync(Property property, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(property);

        if (await store.ExistsAsync(StorageNamespaces.Properties, Key(property.Id), cancellationToken) is false)
        {
            throw new StorageException($"Imovel '{property.Id}' nao encontrado no armazenamento local.");
        }

        await store.SetAsync(StorageNamespaces.Properties, Key(property.Id), PropertyDocument.FromDomain(property), cancellationToken);
        await UpsertIndexAsync(property, cancellationToken);
    }

    public async Task RebuildIndexAsync(CancellationToken cancellationToken = default)
    {
        var ids = await ReadIdsAsync(cancellationToken);
        var entries = new List<PropertyIndexEntry>();

        foreach (var id in ids)
        {
            var document = await store.GetAsync<PropertyDocument>(StorageNamespaces.Properties, $"property-{id}", cancellationToken);
            if (document is not null)
            {
                entries.Add(PropertyIndexEntry.FromDomain(document.ToDomain()));
            }
        }

        await store.SetAsync(StorageNamespaces.Properties, IndexKey, entries, cancellationToken);
    }

    private static string Key(PropertyId id) => $"property-{id.Value}";

    private async Task<List<Guid>> ReadIdsAsync(CancellationToken cancellationToken)
        => await store.GetAsync<List<Guid>>(StorageNamespaces.Properties, IdsKey, cancellationToken) ?? [];

    private async Task<List<PropertyIndexEntry>> ReadIndexAsync(CancellationToken cancellationToken)
        => await store.GetAsync<List<PropertyIndexEntry>>(StorageNamespaces.Properties, IndexKey, cancellationToken) ?? [];

    private async Task UpsertIndexAsync(Property property, CancellationToken cancellationToken)
    {
        var index = await ReadIndexAsync(cancellationToken);

        index.RemoveAll(entry => entry.Id == property.Id.Value);
        index.Add(PropertyIndexEntry.FromDomain(property));

        await store.SetAsync(StorageNamespaces.Properties, IndexKey, index, cancellationToken);
    }
}