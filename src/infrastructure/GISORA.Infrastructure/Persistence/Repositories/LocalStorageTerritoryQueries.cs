using GISORA.Application.Territory;
using GISORA.Domain.Territory;
using GISORA.Infrastructure.Persistence.Documents;

namespace GISORA.Infrastructure.Persistence.Repositories;

public sealed class LocalStorageTerritoryQueries(ILocalStore store) : ITerritoryQueries
{
    private const string MunicipalityIdsKey = "municipality-ids";
    private const string DistrictIndexKey = "district-index";
    private const string NeighborhoodIndexKey = "neighborhood-index";
    private const string FiscalSectorIndexKey = "fiscal-sector-index";

    public async Task<IReadOnlyList<MunicipalityListItem>> ListMunicipalitiesAsync(CancellationToken cancellationToken = default)
    {
        var ids = await store.GetAsync<List<Guid>>(StorageNamespaces.Territory, MunicipalityIdsKey, cancellationToken) ?? [];
        var items = new List<MunicipalityListItem>();

        foreach (var id in ids)
        {
            var document = await store.GetAsync<MunicipalityDocument>(
                StorageNamespaces.Territory,
                $"municipality-{id}",
                cancellationToken);

            if (document is not null)
            {
                items.Add(new MunicipalityListItem(new MunicipalityId(document.Id), document.Name, document.IbgeCodeValue));
            }
        }

        return items.OrderBy(item => item.Name, StringComparer.Ordinal).ToArray();
    }

    public async Task<IReadOnlyList<DistrictListItem>> ListDistrictsAsync(
        MunicipalityId? municipalityId = null,
        CancellationToken cancellationToken = default)
    {
        var index = await store.GetAsync<List<DistrictIndexEntry>>(StorageNamespaces.Territory, DistrictIndexKey, cancellationToken) ?? [];

        return index
            .Where(entry => municipalityId is null || entry.MunicipalityId == municipalityId.Value)
            .OrderBy(entry => entry.Name, StringComparer.Ordinal)
            .Select(entry => new DistrictListItem(new DistrictId(entry.Id), new MunicipalityId(entry.MunicipalityId), entry.Name))
            .ToArray();
    }

    public async Task<IReadOnlyList<NeighborhoodListItem>> ListNeighborhoodsAsync(
        DistrictId? districtId = null,
        CancellationToken cancellationToken = default)
    {
        var index = await store.GetAsync<List<NeighborhoodIndexEntry>>(StorageNamespaces.Territory, NeighborhoodIndexKey, cancellationToken) ?? [];

        return index
            .Where(entry => districtId is null || entry.DistrictId == districtId.Value)
            .OrderBy(entry => entry.Name, StringComparer.Ordinal)
            .Select(entry => new NeighborhoodListItem(new NeighborhoodId(entry.Id), new DistrictId(entry.DistrictId), entry.Name))
            .ToArray();
    }

    public async Task<IReadOnlyList<FiscalSectorListItem>> ListFiscalSectorsAsync(
        DistrictId? districtId = null,
        CancellationToken cancellationToken = default)
    {
        var index = await store.GetAsync<List<FiscalSectorIndexEntry>>(StorageNamespaces.Territory, FiscalSectorIndexKey, cancellationToken) ?? [];

        return index
            .Where(entry => districtId is null || entry.DistrictId == districtId.Value)
            .OrderBy(entry => entry.Code, StringComparer.Ordinal)
            .Select(entry => new FiscalSectorListItem(new FiscalSectorId(entry.Id), new DistrictId(entry.DistrictId), entry.Code))
            .ToArray();
    }
}