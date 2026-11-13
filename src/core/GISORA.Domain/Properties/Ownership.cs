using GISORA.Domain.Common;

namespace GISORA.Domain.Properties;

public sealed class Ownership : Entity<Guid>
{
    internal Ownership(Guid id, TaxpayerId taxpayerId, OwnershipShare share, DateOnly since)
        : base(id)
    {
        TaxpayerId = Guard.NotNull(taxpayerId, nameof(taxpayerId));
        Share = Guard.NotNull(share, nameof(share));
        Since = since;
    }

    public TaxpayerId TaxpayerId { get; }

    public OwnershipShare Share { get; }

    public DateOnly Since { get; }
}