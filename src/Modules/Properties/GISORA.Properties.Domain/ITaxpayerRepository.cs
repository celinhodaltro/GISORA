namespace GISORA.Properties.Domain;

public interface ITaxpayerRepository
{
    Task<Taxpayer?> FindAsync(TaxpayerId id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByDocumentAsync(DocumentNumber document, CancellationToken cancellationToken = default);

    Task AddAsync(Taxpayer taxpayer, CancellationToken cancellationToken = default);

    Task UpdateAsync(Taxpayer taxpayer, CancellationToken cancellationToken = default);
}