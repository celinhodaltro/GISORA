using GISORA.Cemetery.Application;
using GISORA.Cemetery.Domain;
using GISORA.Infrastructure.LocalStorage.Persistence;

namespace GISORA.Infrastructure.LocalStorage.Repositories;

public sealed class LocalStorageCemeteryQueries(ILocalStore store) : ICemeteryQueries
{
    private const string CemeteryIndexKey = "cemetery-index";
    private const string SectorIndexKey = "cemetery-sector-index";
    private const string GraveIndexKey = "grave-index";
    private const string CemeteryIdsKey = "cemetery-ids";
    private const string GraveIdsKey = "grave-ids";

    public async Task<IReadOnlyList<CemeteryListItem>> ListCemeteriesAsync(CancellationToken cancellationToken = default)
    {
        var index = await store.GetAsync<List<CemeteryIndexEntry>>(StorageNamespaces.Cemetery, CemeteryIndexKey, cancellationToken) ?? [];
        var graves = await store.GetAsync<List<GraveIndexEntry>>(StorageNamespaces.Cemetery, GraveIndexKey, cancellationToken) ?? [];

        return index
            .OrderBy(entry => entry.Name, StringComparer.Ordinal)
            .Select(entry => new CemeteryListItem(
                new CemeteryId(entry.Id),
                entry.Name,
                entry.AreaSquareMeters,
                graves.Count(grave => grave.CemeteryId == entry.Id)))
            .ToArray();
    }

    public async Task<IReadOnlyList<CemeterySectorListItem>> ListSectorsAsync(
        CemeteryId? cemeteryId = null,
        CancellationToken cancellationToken = default)
    {
        var index = await store.GetAsync<List<CemeterySectorIndexEntry>>(StorageNamespaces.Cemetery, SectorIndexKey, cancellationToken) ?? [];

        return index
            .Where(entry => cemeteryId is null || entry.CemeteryId == cemeteryId.Value)
            .OrderBy(entry => entry.Code, StringComparer.Ordinal)
            .Select(entry => new CemeterySectorListItem(
                new CemeterySectorId(entry.Id),
                new CemeteryId(entry.CemeteryId),
                entry.Code))
            .ToArray();
    }

    public async Task<IReadOnlyList<GraveListItem>> ListGravesAsync(
        CemeterySectorId? sectorId = null,
        CancellationToken cancellationToken = default)
    {
        var index = await store.GetAsync<List<GraveIndexEntry>>(StorageNamespaces.Cemetery, GraveIndexKey, cancellationToken) ?? [];

        return index
            .Where(entry => sectorId is null || entry.SectorId == sectorId.Value)
            .OrderBy(entry => entry.Code, StringComparer.Ordinal)
            .Select(entry => new GraveListItem(
                new GraveId(entry.Id),
                new CemeterySectorId(entry.SectorId),
                entry.Code,
                entry.IsOccupied,
                entry.ActiveBurialId,
                entry.ActiveBurialPerson))
            .ToArray();
    }

    public async Task<IReadOnlyList<CemeteryMapItem>> GetCemeteryMapItemsAsync(CancellationToken cancellationToken = default)
    {
        var ids = await store.GetAsync<List<Guid>>(StorageNamespaces.Cemetery, CemeteryIdsKey, cancellationToken) ?? [];
        var items = new List<CemeteryMapItem>();

        foreach (var id in ids)
        {
            var document = await store.GetAsync<CemeteryDocument>(StorageNamespaces.Cemetery, $"cemetery-{id}", cancellationToken);
            if (document is null)
            {
                continue;
            }

            var cemetery = document.ToDomain();
            items.Add(new CemeteryMapItem(cemetery.Id, cemetery.Name, cemetery.Boundary));
        }

        return items;
    }

    public async Task<IReadOnlyList<GraveMapItem>> GetGraveMapItemsAsync(CancellationToken cancellationToken = default)
    {
        var ids = await store.GetAsync<List<Guid>>(StorageNamespaces.Cemetery, GraveIdsKey, cancellationToken) ?? [];
        var items = new List<GraveMapItem>();

        foreach (var id in ids)
        {
            var document = await store.GetAsync<GraveDocument>(StorageNamespaces.Cemetery, $"grave-{id}", cancellationToken);
            if (document is null)
            {
                continue;
            }

            var grave = document.ToDomain();
            items.Add(new GraveMapItem(grave.Id, grave.Code, grave.IsOccupied, grave.Location));
        }

        return items;
    }
}