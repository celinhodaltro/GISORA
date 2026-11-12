using GISORA.BuildingBlocks.Domain;
using GISORA.Geospatial.Domain;

namespace GISORA.Properties.Domain;

public enum UnitUsage
{
    Residential,
    Commercial,
    Service,
    Mixed,
    Other,
}

public sealed class Unit : Entity<Guid>
{
    internal Unit(Guid id, string registration, Area area, UnitUsage usage)
        : base(id)
    {
        Guard.Condition(area.SquareMeters > 0, "Area da unidade deve ser maior que zero.", "properties.unit.area");

        Registration = Guard.NotNullOrWhiteSpace(registration, nameof(registration)).Trim();
        Area = area;
        Usage = usage;
    }

    public string Registration { get; }

    public Area Area { get; }

    public UnitUsage Usage { get; }
}