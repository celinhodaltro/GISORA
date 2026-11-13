using GISORA.Domain.UrbanPlanning;
using GISORA.Infrastructure.Persistence.Documents;

namespace GISORA.Infrastructure.Persistence.Repositories;

public sealed class LocalStorageZoneRepository(ILocalStore store) : IZoneRepository
{
    private const string IndexKey = "zone-index";
    private const string IdsKey = "zone-ids";

    public async Task<Zone?> FindAsync(ZoneId id, CancellationToken cancellationToken = default)
    {
        var document = await store.GetAsync<ZoneDocument>(StorageNamespaces.UrbanPlanning, $"zone-{id.Value}", cancellationToken);

        return document?.ToDomain();
    }

    public async Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var index = await ReadIndexAsync(cancellationToken);
        var normalized = code.Trim().ToUpperInvariant();

        return index.Any(entry => string.Equals(entry.Code, normalized, StringComparison.Ordinal));
    }

    public async Task AddAsync(Zone zone, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(zone);

        await SaveAsync(zone, cancellationToken);

        var ids = await ReadIdsAsync(cancellationToken);
        if (ids.Contains(zone.Id.Value) is false)
        {
            ids.Add(zone.Id.Value);
            await store.SetAsync(StorageNamespaces.UrbanPlanning, IdsKey, ids, cancellationToken);
        }
    }

    public async Task UpdateAsync(Zone zone, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(zone);

        if (await store.ExistsAsync(StorageNamespaces.UrbanPlanning, $"zone-{zone.Id.Value}", cancellationToken) is false)
        {
            throw new StorageException($"Zona '{zone.Id}' nao encontrada no armazenamento local.");
        }

        await SaveAsync(zone, cancellationToken);
    }

    private async Task SaveAsync(Zone zone, CancellationToken cancellationToken)
    {
        await store.SetAsync(
            StorageNamespaces.UrbanPlanning,
            $"zone-{zone.Id.Value}",
            ZoneDocument.FromDomain(zone),
            cancellationToken);

        var index = await ReadIndexAsync(cancellationToken);
        index.RemoveAll(entry => entry.Id == zone.Id.Value);
        index.Add(new ZoneIndexEntry
        {
            Id = zone.Id.Value,
            Code = zone.Code,
            Name = zone.Name,
            MaxOccupancyPercent = zone.Parameters.MaxOccupancyPercent,
            MaxHeightMeters = zone.Parameters.MaxHeightMeters,
            MinSetbackMeters = zone.Parameters.MinSetbackMeters,
            AllowedUses = zone.Parameters.AllowedUses,
            AreaSquareMeters = zone.Area.SquareMeters,
        });

        await store.SetAsync(StorageNamespaces.UrbanPlanning, IndexKey, index, cancellationToken);
    }

    private async Task<List<Guid>> ReadIdsAsync(CancellationToken cancellationToken)
        => await store.GetAsync<List<Guid>>(StorageNamespaces.UrbanPlanning, IdsKey, cancellationToken) ?? [];

    private async Task<List<ZoneIndexEntry>> ReadIndexAsync(CancellationToken cancellationToken)
        => await store.GetAsync<List<ZoneIndexEntry>>(StorageNamespaces.UrbanPlanning, IndexKey, cancellationToken) ?? [];
}

public sealed class LocalStorageRestrictionRepository(ILocalStore store) : IRestrictionRepository
{
    private const string IdsKey = "restriction-ids";

    public async Task AddAsync(Restriction restriction, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(restriction);

        await store.SetAsync(
            StorageNamespaces.UrbanPlanning,
            $"restriction-{restriction.Id.Value}",
            RestrictionDocument.FromDomain(restriction),
            cancellationToken);

        var ids = await store.GetAsync<List<Guid>>(StorageNamespaces.UrbanPlanning, IdsKey, cancellationToken) ?? [];
        if (ids.Contains(restriction.Id.Value) is false)
        {
            ids.Add(restriction.Id.Value);
            await store.SetAsync(StorageNamespaces.UrbanPlanning, IdsKey, ids, cancellationToken);
        }
    }
}