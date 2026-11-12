using GISORA.Infrastructure.LocalStorage.Persistence;
using GISORA.PublicFacilities.Domain;

namespace GISORA.Infrastructure.LocalStorage.Repositories;

public sealed class LocalStorageFacilityRepository(ILocalStore store) : IPublicFacilityRepository
{
    private const string IndexKey = "facility-index";
    private const string IdsKey = "facility-ids";

    public async Task<PublicFacility?> FindAsync(PublicFacilityId id, CancellationToken cancellationToken = default)
    {
        var document = await store.GetAsync<FacilityDocument>(StorageNamespaces.Facilities, Key(id), cancellationToken);

        return document?.ToDomain();
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid? neighborhoodId, CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim().ToUpperInvariant();
        var index = await ReadIndexAsync(cancellationToken);

        return index.Any(entry => entry.NameSearch == normalized && entry.NeighborhoodId == neighborhoodId);
    }

    public async Task AddAsync(PublicFacility facility, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(facility);

        var index = await ReadIndexAsync(cancellationToken);
        var normalized = facility.Name.ToUpperInvariant();

        if (index.Any(entry => entry.NameSearch == normalized && entry.NeighborhoodId == facility.NeighborhoodId))
        {
            throw new StorageException("Ja existe equipamento com este nome no armazenamento local.");
        }

        await store.SetAsync(StorageNamespaces.Facilities, Key(facility.Id), FacilityDocument.FromDomain(facility), cancellationToken);

        var ids = await ReadIdsAsync(cancellationToken);
        if (ids.Contains(facility.Id.Value) is false)
        {
            ids.Add(facility.Id.Value);
            await store.SetAsync(StorageNamespaces.Facilities, IdsKey, ids, cancellationToken);
        }

        index.Add(FacilityIndexEntry.FromDomain(facility));
        await store.SetAsync(StorageNamespaces.Facilities, IndexKey, index, cancellationToken);
    }

    public async Task UpdateAsync(PublicFacility facility, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(facility);

        if (await store.ExistsAsync(StorageNamespaces.Facilities, Key(facility.Id), cancellationToken) is false)
        {
            throw new StorageException($"Equipamento '{facility.Id}' nao encontrado no armazenamento local.");
        }

        await store.SetAsync(StorageNamespaces.Facilities, Key(facility.Id), FacilityDocument.FromDomain(facility), cancellationToken);

        var index = await ReadIndexAsync(cancellationToken);
        index.RemoveAll(entry => entry.Id == facility.Id.Value);
        index.Add(FacilityIndexEntry.FromDomain(facility));
        await store.SetAsync(StorageNamespaces.Facilities, IndexKey, index, cancellationToken);
    }

    private static string Key(PublicFacilityId id) => $"facility-{id.Value}";

    private async Task<List<Guid>> ReadIdsAsync(CancellationToken cancellationToken)
        => await store.GetAsync<List<Guid>>(StorageNamespaces.Facilities, IdsKey, cancellationToken) ?? [];

    private async Task<List<FacilityIndexEntry>> ReadIndexAsync(CancellationToken cancellationToken)
        => await store.GetAsync<List<FacilityIndexEntry>>(StorageNamespaces.Facilities, IndexKey, cancellationToken) ?? [];
}