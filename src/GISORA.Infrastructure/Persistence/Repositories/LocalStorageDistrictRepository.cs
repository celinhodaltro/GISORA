using GISORA.Domain.Territory;
using GISORA.Infrastructure.Persistence.Documents;

namespace GISORA.Infrastructure.Persistence.Repositories;

public sealed class LocalStorageDistrictRepository(ILocalStore store) : IDistrictRepository
{
    private const string IndexKey = "district-index";
    private const string IdsKey = "district-ids";

    public async Task<District?> FindAsync(DistrictId id, CancellationToken cancellationToken = default)
    {
        var document = await store.GetAsync<DistrictDocument>(StorageNamespaces.Territory, Key(id), cancellationToken);

        return document?.ToDomain();
    }

    public async Task<bool> ExistsByNameAsync(MunicipalityId municipalityId, string name, CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim().ToUpperInvariant();
        var index = await ReadIndexAsync(cancellationToken);

        return index.Any(entry => entry.MunicipalityId == municipalityId.Value && entry.NameSearch == normalized);
    }

    public async Task AddAsync(District district, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(district);

        var index = await ReadIndexAsync(cancellationToken);
        var normalized = district.Name.ToUpperInvariant();

        if (index.Any(entry => entry.MunicipalityId == district.MunicipalityId.Value && entry.NameSearch == normalized))
        {
            throw new StorageException("Ja existe distrito com este nome no armazenamento local.");
        }

        await store.SetAsync(StorageNamespaces.Territory, Key(district.Id), DistrictDocument.FromDomain(district), cancellationToken);

        var ids = await ReadIdsAsync(cancellationToken);
        if (ids.Contains(district.Id.Value) is false)
        {
            ids.Add(district.Id.Value);
            await store.SetAsync(StorageNamespaces.Territory, IdsKey, ids, cancellationToken);
        }

        index.Add(new DistrictIndexEntry
        {
            Id = district.Id.Value,
            MunicipalityId = district.MunicipalityId.Value,
            Name = district.Name,
            NameSearch = normalized,
        });
        await store.SetAsync(StorageNamespaces.Territory, IndexKey, index, cancellationToken);
    }

    public async Task UpdateAsync(District district, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(district);

        if (await store.ExistsAsync(StorageNamespaces.Territory, Key(district.Id), cancellationToken) is false)
        {
            throw new StorageException($"Distrito '{district.Id}' nao encontrado no armazenamento local.");
        }

        await store.SetAsync(StorageNamespaces.Territory, Key(district.Id), DistrictDocument.FromDomain(district), cancellationToken);

        var index = await ReadIndexAsync(cancellationToken);
        index.RemoveAll(entry => entry.Id == district.Id.Value);
        index.Add(new DistrictIndexEntry
        {
            Id = district.Id.Value,
            MunicipalityId = district.MunicipalityId.Value,
            Name = district.Name,
            NameSearch = district.Name.ToUpperInvariant(),
        });
        await store.SetAsync(StorageNamespaces.Territory, IndexKey, index, cancellationToken);
    }

    private static string Key(DistrictId id) => $"district-{id.Value}";

    private async Task<List<Guid>> ReadIdsAsync(CancellationToken cancellationToken)
        => await store.GetAsync<List<Guid>>(StorageNamespaces.Territory, IdsKey, cancellationToken) ?? [];

    private async Task<List<DistrictIndexEntry>> ReadIndexAsync(CancellationToken cancellationToken)
        => await store.GetAsync<List<DistrictIndexEntry>>(StorageNamespaces.Territory, IndexKey, cancellationToken) ?? [];
}