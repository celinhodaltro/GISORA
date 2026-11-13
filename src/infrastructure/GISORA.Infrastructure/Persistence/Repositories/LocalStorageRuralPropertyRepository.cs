using GISORA.Domain.Rural;
using GISORA.Infrastructure.Persistence.Documents;

namespace GISORA.Infrastructure.Persistence.Repositories;

public sealed class LocalStorageRuralPropertyRepository(ILocalStore store) : IRuralPropertyRepository
{
    private const string IndexKey = "rural-index";
    private const string IdsKey = "rural-ids";

    public async Task<RuralProperty?> FindAsync(RuralPropertyId id, CancellationToken cancellationToken = default)
    {
        var document = await store.GetAsync<RuralPropertyDocument>(
            StorageNamespaces.Rural,
            $"rural-{id.Value}",
            cancellationToken);

        return document?.ToDomain();
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var index = await ReadIndexAsync(cancellationToken);
        var normalized = name.Trim().ToLowerInvariant();

        return index.Any(entry => entry.NameSearch == normalized);
    }

    public async Task AddAsync(RuralProperty property, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(property);

        await SaveDocumentAsync(property, cancellationToken);

        var ids = await ReadIdsAsync(cancellationToken);
        if (ids.Contains(property.Id.Value) is false)
        {
            ids.Add(property.Id.Value);
            await store.SetAsync(StorageNamespaces.Rural, IdsKey, ids, cancellationToken);
        }
    }

    public async Task UpdateAsync(RuralProperty property, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(property);

        if (await store.ExistsAsync(StorageNamespaces.Rural, $"rural-{property.Id.Value}", cancellationToken) is false)
        {
            throw new StorageException($"Propriedade rural '{property.Id}' nao encontrada no armazenamento local.");
        }

        await SaveDocumentAsync(property, cancellationToken);
    }

    private async Task SaveDocumentAsync(RuralProperty property, CancellationToken cancellationToken)
    {
        await store.SetAsync(
            StorageNamespaces.Rural,
            $"rural-{property.Id.Value}",
            RuralPropertyDocument.FromDomain(property),
            cancellationToken);

        var index = await ReadIndexAsync(cancellationToken);
        index.RemoveAll(entry => entry.Id == property.Id.Value);
        index.Add(new RuralPropertyIndexEntry
        {
            Id = property.Id.Value,
            Name = property.Name,
            NameSearch = property.Name.Trim().ToLowerInvariant(),
            Usage = property.Usage,
            CarCode = property.CarCode,
            AreaSquareMeters = property.Area.SquareMeters,
            ConstrainedSquareMeters = property.ConstrainedSquareMeters,
            PreservationAreaCount = property.PreservationAreas.Count,
            SourceKind = property.Source.Kind,
        });

        await store.SetAsync(StorageNamespaces.Rural, IndexKey, index, cancellationToken);
    }

    private async Task<List<Guid>> ReadIdsAsync(CancellationToken cancellationToken)
        => await store.GetAsync<List<Guid>>(StorageNamespaces.Rural, IdsKey, cancellationToken) ?? [];

    private async Task<List<RuralPropertyIndexEntry>> ReadIndexAsync(CancellationToken cancellationToken)
        => await store.GetAsync<List<RuralPropertyIndexEntry>>(StorageNamespaces.Rural, IndexKey, cancellationToken) ?? [];
}