using GISORA.BuildingBlocks.Domain;

namespace GISORA.Territory.Domain;

public sealed class FiscalSector : AggregateRoot<FiscalSectorId>
{
    private FiscalSector(FiscalSectorId id, DistrictId districtId, string code)
        : base(id)
    {
        DistrictId = Guard.NotNull(districtId, nameof(districtId));
        Code = Guard.NotNullOrWhiteSpace(code, nameof(code)).Trim();
    }

    public DistrictId DistrictId { get; }

    public string Code { get; }

    public static FiscalSector Create(DistrictId districtId, string code)
        => new(FiscalSectorId.New(), districtId, code);

    internal static FiscalSector Rehydrate(FiscalSectorId id, DistrictId districtId, string code)
        => new(id, districtId, code);
}