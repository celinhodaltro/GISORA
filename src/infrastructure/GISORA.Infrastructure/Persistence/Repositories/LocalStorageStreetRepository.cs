using GISORA.Domain.Roads;
using GISORA.Infrastructure.Persistence.Documents;

namespace GISORA.Infrastructure.Persistence.Repositories;

public sealed class LocalStorageStreetRepository(ILocalStore store) : IStreetRepository
{
    private const string IndexKey = "street-index";
    private const string IdsKey = "street-ids";

    public async Task<Street?> FindAsync(StreetId id, CancellationToken cancellationToken = default)
    {
        var document = await store.GetAsync<StreetDocument>(StorageNamespaces.Roads, Key(id), cancellationToken);

        return document?.ToDomain();
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid? neighborhoodId, CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim().ToUpperInvariant();
        var index = await ReadIndexAsync(cancellationToken);

        return index.Any(entry =>
            entry.NameSearch == normalized
            && entry.NeighborhoodId == neighborhoodId);
    }

    public async Task AddAsync(Street street, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(street);

        var index = await ReadIndexAsync(cancellationToken);
        var normalized = street.Name.ToUpperInvariant();

        if (index.Any(entry => entry.NameSearch == normalized && entry.NeighborhoodId == street.NeighborhoodId))
        {
            throw new StorageException("Ja existe logradouro com este nome no armazenamento local.");
        }

        await store.SetAsync(StorageNamespaces.Roads, Key(street.Id), StreetDocument.FromDomain(street), cancellationToken);

        var ids = await ReadIdsAsync(cancellationToken);
        if (ids.Contains(street.Id.Value) is false)
        {
            ids.Add(street.Id.Value);
            await store.SetAsync(StorageNamespaces.Roads, IdsKey, ids, cancellationToken);
        }

        index.Add(StreetIndexEntry.FromDomain(street));
        await store.SetAsync(StorageNamespaces.Roads, IndexKey, index, cancellationToken);
    }

    public async Task UpdateAsync(Street street, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(street);

        if (await store.ExistsAsync(StorageNamespaces.Roads, Key(street.Id), cancellationToken) is false)
        {
            throw new StorageException($"Logradouro '{street.Id}' nao encontrado no armazenamento local.");
        }

        await store.SetAsync(StorageNamespaces.Roads, Key(street.Id), StreetDocument.FromDomain(street), cancellationToken);

        var index = await ReadIndexAsync(cancellationToken);
        index.RemoveAll(entry => entry.Id == street.Id.Value);
        index.Add(StreetIndexEntry.FromDomain(street));
        await store.SetAsync(StorageNamespaces.Roads, IndexKey, index, cancellationToken);
    }

    private static string Key(StreetId id) => $"street-{id.Value}";

    private async Task<List<Guid>> ReadIdsAsync(CancellationToken cancellationToken)
        => await store.GetAsync<List<Guid>>(StorageNamespaces.Roads, IdsKey, cancellationToken) ?? [];

    private async Task<List<StreetIndexEntry>> ReadIndexAsync(CancellationToken cancellationToken)
        => await store.GetAsync<List<StreetIndexEntry>>(StorageNamespaces.Roads, IndexKey, cancellationToken) ?? [];
}