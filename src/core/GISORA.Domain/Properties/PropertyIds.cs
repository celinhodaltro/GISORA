using GISORA.Domain.Common;

namespace GISORA.Domain.Properties;

public sealed record PropertyId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static PropertyId New() => new(Guid.CreateVersion7());
}

public sealed record TaxpayerId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static TaxpayerId New() => new(Guid.CreateVersion7());
}