using GISORA.BuildingBlocks.Domain;

namespace GISORA.Properties.Domain;

public sealed class Taxpayer : AggregateRoot<TaxpayerId>
{
    private Taxpayer(TaxpayerId id, string name, DocumentNumber document)
        : base(id)
    {
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name)).Trim();
        Document = Guard.NotNull(document, nameof(document));
    }

    public string Name { get; private set; }

    public DocumentNumber Document { get; }

    public static Taxpayer Register(string name, DocumentNumber document)
        => new(TaxpayerId.New(), name, document);

    public void Rename(string name)
        => Name = Guard.NotNullOrWhiteSpace(name, nameof(name)).Trim();

    internal static Taxpayer Rehydrate(TaxpayerId id, string name, DocumentNumber document)
        => new(id, name, document);
}