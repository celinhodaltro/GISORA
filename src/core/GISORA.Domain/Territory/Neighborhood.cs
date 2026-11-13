using GISORA.Domain.Common;

namespace GISORA.Domain.Territory;

public sealed class Neighborhood : AggregateRoot<NeighborhoodId>
{
    private Neighborhood(NeighborhoodId id, DistrictId districtId, string name)
        : base(id)
    {
        DistrictId = Guard.NotNull(districtId, nameof(districtId));
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name)).Trim();
    }

    public DistrictId DistrictId { get; }

    public string Name { get; }

    public static Neighborhood Create(DistrictId districtId, string name)
        => new(NeighborhoodId.New(), districtId, name);

    internal static Neighborhood Rehydrate(NeighborhoodId id, DistrictId districtId, string name)
        => new(id, districtId, name);
}