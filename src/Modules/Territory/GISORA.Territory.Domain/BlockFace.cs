using GISORA.BuildingBlocks.Domain;
using GISORA.Geospatial.Domain;

namespace GISORA.Territory.Domain;

public sealed class BlockFace : Entity<Guid>
{
    internal BlockFace(Guid id, Guid streetId, LineString geometry, Distance frontage)
        : base(id)
    {
        Guard.NotEmpty(streetId, nameof(streetId));
        Guard.Condition(frontage.Meters > 0, "Testada deve ser maior que zero.", "territory.blockface.frontage");

        StreetId = streetId;
        Geometry = Guard.NotNull(geometry, nameof(geometry));
        Frontage = frontage;
    }

    public Guid StreetId { get; }

    public LineString Geometry { get; }

    public Distance Frontage { get; }
}