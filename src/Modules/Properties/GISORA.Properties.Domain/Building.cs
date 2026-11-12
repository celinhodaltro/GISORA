using GISORA.BuildingBlocks.Domain;
using GISORA.Geospatial.Domain;

namespace GISORA.Properties.Domain;

public enum BuildingType
{
    Residential,
    Commercial,
    Industrial,
    Mixed,
    Institutional,
    Other,
}

public sealed class Building : Entity<Guid>
{
    internal Building(Guid id, BuildingType type, Area area, int floors)
        : base(id)
    {
        Guard.Condition(area.SquareMeters > 0, "Area construida deve ser maior que zero.", "properties.building.area");
        Guard.Positive(floors, nameof(floors));

        Type = type;
        Area = area;
        Floors = floors;
    }

    public BuildingType Type { get; }

    public Area Area { get; }

    public int Floors { get; }
}