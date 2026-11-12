using GISORA.Infrastructure.LocalStorage.Persistence;
using GISORA.Properties.Domain;

namespace GISORA.Infrastructure.LocalStorage.Repositories;

public sealed class LocalStorageTaxpayerRepository(ILocalStore store) : ITaxpayerRepository
{
    private const string DocumentIndexKey = "taxpayer-document-index";

    public async Task<Taxpayer?> FindAsync(TaxpayerId id, CancellationToken cancellationToken = default)
    {
        var document = await store.GetAsync<TaxpayerDocument>(StorageNamespaces.Taxpayers, Key(id), cancellationToken);

        return document?.ToDomain();
    }

    public async Task<bool> ExistsByDocumentAsync(DocumentNumber document, CancellationToken cancellationToken = default)
    {
        var index = await ReadDocumentIndexAsync(cancellationToken);

        return index.ContainsKey(document.Digits);
    }

    public async Task AddAsync(Taxpayer taxpayer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(taxpayer);

        var index = await ReadDocumentIndexAsync(cancellationToken);
        if (index.ContainsKey(taxpayer.Document.Digits))
        {
            throw new StorageException("Ja existe contribuinte com este documento no armazenamento local.");
        }

        await store.SetAsync(StorageNamespaces.Taxpayers, Key(taxpayer.Id), TaxpayerDocument.FromDomain(taxpayer), cancellationToken);

        index[taxpayer.Document.Digits] = taxpayer.Id.Value;
        await store.SetAsync(StorageNamespaces.Taxpayers, DocumentIndexKey, index, cancellationToken);
    }

    public async Task UpdateAsync(Taxpayer taxpayer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(taxpayer);

        if (await store.ExistsAsync(StorageNamespaces.Taxpayers, Key(taxpayer.Id), cancellationToken) is false)
        {
            throw new StorageException($"Contribuinte '{taxpayer.Id}' nao encontrado no armazenamento local.");
        }

        await store.SetAsync(StorageNamespaces.Taxpayers, Key(taxpayer.Id), TaxpayerDocument.FromDomain(taxpayer), cancellationToken);

        var index = await ReadDocumentIndexAsync(cancellationToken);
        index[taxpayer.Document.Digits] = taxpayer.Id.Value;
        await store.SetAsync(StorageNamespaces.Taxpayers, DocumentIndexKey, index, cancellationToken);
    }

    private static string Key(TaxpayerId id) => $"taxpayer-{id.Value}";

    private async Task<Dictionary<string, Guid>> ReadDocumentIndexAsync(CancellationToken cancellationToken)
        => await store.GetAsync<Dictionary<string, Guid>>(StorageNamespaces.Taxpayers, DocumentIndexKey, cancellationToken) ?? [];
}