using GISORA.Infrastructure.LocalStorage.Persistence;
using GISORA.Properties.Application;

namespace GISORA.Infrastructure.LocalStorage.Repositories;

public sealed class LocalStoragePropertyMapQueries(ILocalStore store) : IPropertyMapQueries
{
    private const string IdsKey = "property-ids";

    public async Task<IReadOnlyList<PropertyMapItem>> GetMapItemsAsync(CancellationToken cancellationToken = default)
    {
        var ids = await store.GetAsync<List<Guid>>(StorageNamespaces.Properties, IdsKey, cancellationToken) ?? [];
        var items = new List<PropertyMapItem>();

        foreach (var id in ids)
        {
            var document = await store.GetAsync<PropertyDocument>(
                StorageNamespaces.Properties,
                $"property-{id}",
                cancellationToken);

            if (document is null || document.Geometry is null)
            {
                continue;
            }

            var property = document.ToDomain();

            items.Add(new PropertyMapItem(
                property.Id,
                property.Registration.Formatted,
                property.LandArea.SquareMeters,
                property.BuiltArea.SquareMeters,
                property.Geometry!));
        }

        return items;
    }
}