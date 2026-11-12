using GISORA.BuildingBlocks.Domain;

namespace GISORA.Territory.Domain;

public sealed class District : AggregateRoot<DistrictId>
{
    private District(DistrictId id, MunicipalityId municipalityId, string name)
        : base(id)
    {
        MunicipalityId = Guard.NotNull(municipalityId, nameof(municipalityId));
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name)).Trim();
    }

    public MunicipalityId MunicipalityId { get; }

    public string Name { get; }

    public static District Create(MunicipalityId municipalityId, string name)
        => new(DistrictId.New(), municipalityId, name);

    internal static District Rehydrate(DistrictId id, MunicipalityId municipalityId, string name)
        => new(id, municipalityId, name);
}