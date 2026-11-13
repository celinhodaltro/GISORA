using GISORA.Geospatial.Application;
using GISORA.Geospatial.Domain;
using GISORA.Infrastructure.LocalStorage.Persistence;
using GISORA.UrbanPlanning.Application;

namespace GISORA.Infrastructure.LocalStorage.Repositories;

public sealed class LocalStorageRegulationQueries(ILocalStore store, ISpatialOperations spatial) : IRegulationQueries
{
    private const string ZoneIndexKey = "zone-index";
    private const string ZoneIdsKey = "zone-ids";
    private const string RestrictionIdsKey = "restriction-ids";

    public async Task<IReadOnlyList<ZoneListItem>> ListZonesAsync(CancellationToken cancellationToken = default)
    {
        var index = await store.GetAsync<List<ZoneIndexEntry>>(StorageNamespaces.UrbanPlanning, ZoneIndexKey, cancellationToken) ?? [];

        return index
            .OrderBy(entry => entry.Code, StringComparer.Ordinal)
            .Select(entry => new ZoneListItem(
                new UrbanPlanning.Domain.ZoneId(entry.Id),
                entry.Code,
                entry.Name,
                entry.MaxOccupancyPercent,
                entry.MaxHeightMeters,
                entry.MinSetbackMeters,
                entry.AllowedUses,
                entry.AreaSquareMeters / 10_000m))
            .ToArray();
    }

    public async Task<IReadOnlyList<ZoneMapItem>> GetZoneMapItemsAsync(CancellationToken cancellationToken = default)
    {
        var zones = await ReadZonesAsync(cancellationToken);

        return zones
            .Select(zone => new ZoneMapItem(zone.Id, zone.Code, zone.Name, zone.Boundary))
            .ToArray();
    }

    public async Task<IReadOnlyList<RestrictionMapItem>> GetRestrictionMapItemsAsync(CancellationToken cancellationToken = default)
    {
        var restrictions = await ReadRestrictionsAsync(cancellationToken);

        return restrictions
            .Select(restriction => new RestrictionMapItem(
                restriction.Id,
                restriction.Type,
                restriction.Description,
                restriction.Boundary))
            .ToArray();
    }

    public async Task<IncidentRegulation> FindIncidentAsync(Geometry geometry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(geometry);

        var zones = await ReadZonesAsync(cancellationToken);
        var restrictions = await ReadRestrictionsAsync(cancellationToken);
        var incidentZones = new List<ZoneListItem>();
        var incidentRestrictions = new List<RestrictionMapItem>();

        foreach (var zone in zones)
        {
            if (await spatial.ContainsAsync(zone.Boundary, geometry, cancellationToken))
            {
                incidentZones.Add(new ZoneListItem(
                    zone.Id,
                    zone.Code,
                    zone.Name,
                    zone.Parameters.MaxOccupancyPercent,
                    zone.Parameters.MaxHeightMeters,
                    zone.Parameters.MinSetbackMeters,
                    zone.Parameters.AllowedUses,
                    zone.Area.Hectares));
            }
        }

        foreach (var restriction in restrictions)
        {
            if (await spatial.IntersectsAsync(restriction.Boundary, geometry, cancellationToken))
            {
                incidentRestrictions.Add(new RestrictionMapItem(
                    restriction.Id,
                    restriction.Type,
                    restriction.Description,
                    restriction.Boundary));
            }
        }

        return new IncidentRegulation(incidentZones, incidentRestrictions);
    }

    private async Task<IReadOnlyList<UrbanPlanning.Domain.Zone>> ReadZonesAsync(CancellationToken cancellationToken)
    {
        var ids = await store.GetAsync<List<Guid>>(StorageNamespaces.UrbanPlanning, ZoneIdsKey, cancellationToken) ?? [];
        var zones = new List<UrbanPlanning.Domain.Zone>(ids.Count);

        foreach (var id in ids)
        {
            var document = await store.GetAsync<ZoneDocument>(StorageNamespaces.UrbanPlanning, $"zone-{id}", cancellationToken);

            if (document is not null)
            {
                zones.Add(document.ToDomain());
            }
        }

        return zones;
    }

    private async Task<IReadOnlyList<UrbanPlanning.Domain.Restriction>> ReadRestrictionsAsync(CancellationToken cancellationToken)
    {
        var ids = await store.GetAsync<List<Guid>>(StorageNamespaces.UrbanPlanning, RestrictionIdsKey, cancellationToken) ?? [];
        var items = new List<UrbanPlanning.Domain.Restriction>(ids.Count);

        foreach (var id in ids)
        {
            var document = await store.GetAsync<RestrictionDocument>(
                StorageNamespaces.UrbanPlanning,
                $"restriction-{id}",
                cancellationToken);

            if (document is not null)
            {
                items.Add(document.ToDomain());
            }
        }

        return items;
    }
}