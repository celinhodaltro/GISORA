using GISORA.Domain.Common;

namespace GISORA.Domain.Territory;

public sealed record MunicipalityId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static MunicipalityId New() => new(Guid.CreateVersion7());
}

public sealed record DistrictId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static DistrictId New() => new(Guid.CreateVersion7());
}

public sealed record NeighborhoodId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static NeighborhoodId New() => new(Guid.CreateVersion7());
}

public sealed record FiscalSectorId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static FiscalSectorId New() => new(Guid.CreateVersion7());
}

public sealed record UrbanBlockId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static UrbanBlockId New() => new(Guid.CreateVersion7());
}