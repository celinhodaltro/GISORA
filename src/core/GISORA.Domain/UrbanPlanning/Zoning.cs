using GISORA.Domain.Common;
using GISORA.Domain.Geospatial;

namespace GISORA.Domain.UrbanPlanning;

public enum RestrictionType
{
    RiskArea,
    UrbanPreservationArea,
    Setback,
    HeightLimit,
}

public sealed record ZoneId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static ZoneId New() => new(Guid.CreateVersion7());
}

public sealed record RestrictionId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static RestrictionId New() => new(Guid.CreateVersion7());
}

public sealed record ZoneDefined(ZoneId ZoneId, string Code) : DomainEvent;

public sealed record RestrictionDefined(RestrictionId RestrictionId, RestrictionType Type) : DomainEvent;

public sealed class ZoneParameters : ValueObject
{
    private ZoneParameters(
        decimal? maxOccupancyPercent,
        decimal? maxHeightMeters,
        decimal? minSetbackMeters,
        string? allowedUses)
    {
        Guard.Condition(
            maxOccupancyPercent is not null || maxHeightMeters is not null || minSetbackMeters is not null || string.IsNullOrWhiteSpace(allowedUses) is false,
            "Zona exige ao menos um parametro de uso ou ocupacao.",
            "urban.zone.parameters");

        Guard.Condition(
            maxOccupancyPercent is null or > 0m and <= 100m,
            "Taxa de ocupacao deve estar entre 0 e 100.",
            "urban.zone.occupancy");

        Guard.Condition(maxHeightMeters is null or > 0m, "Gabarito deve ser maior que zero.", "urban.zone.height");
        Guard.Condition(minSetbackMeters is null or >= 0m, "Recuo nao pode ser negativo.", "urban.zone.setback");

        MaxOccupancyPercent = maxOccupancyPercent;
        MaxHeightMeters = maxHeightMeters;
        MinSetbackMeters = minSetbackMeters;
        AllowedUses = string.IsNullOrWhiteSpace(allowedUses) ? null : allowedUses.Trim();
    }

    public decimal? MaxOccupancyPercent { get; }

    public decimal? MaxHeightMeters { get; }

    public decimal? MinSetbackMeters { get; }

    public string? AllowedUses { get; }

    public static ZoneParameters Create(
        decimal? maxOccupancyPercent = null,
        decimal? maxHeightMeters = null,
        decimal? minSetbackMeters = null,
        string? allowedUses = null)
        => new(maxOccupancyPercent, maxHeightMeters, minSetbackMeters, allowedUses);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return MaxOccupancyPercent;
        yield return MaxHeightMeters;
        yield return MinSetbackMeters;
        yield return AllowedUses;
    }
}

public sealed class Zone : AggregateRoot<ZoneId>
{
    private Zone(ZoneId id, string code, string name, ZoneParameters parameters, Polygon boundary, Area area)
        : base(id)
    {
        Code = NormalizeCode(code);
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name)).Trim();
        Parameters = Guard.NotNull(parameters, nameof(parameters));
        Boundary = Guard.NotNull(boundary, nameof(boundary));
        Area = Guard.NotNull(area, nameof(area));

        Guard.Condition(area.SquareMeters > 0, "Area da zona deve ser maior que zero.", "urban.zone.area");
    }

    public string Code { get; private set; }

    public string Name { get; private set; }

    public ZoneParameters Parameters { get; private set; }

    public Polygon Boundary { get; private set; }

    public Area Area { get; private set; }

    public static Zone Define(string code, string name, ZoneParameters parameters, Polygon boundary, Area area)
    {
        var zone = new Zone(ZoneId.New(), code, name, parameters, boundary, area);
        zone.Raise(new ZoneDefined(zone.Id, zone.Code));

        return zone;
    }

    public void Update(string code, string name, ZoneParameters parameters)
    {
        Code = NormalizeCode(code);
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name)).Trim();
        Parameters = Guard.NotNull(parameters, nameof(parameters));
    }

    internal static Zone Rehydrate(ZoneId id, string code, string name, ZoneParameters parameters, Polygon boundary, Area area)
        => new(id, code, name, parameters, boundary, area);

    private static string NormalizeCode(string code)
    {
        var normalized = Guard.NotNullOrWhiteSpace(code, nameof(code)).Trim().ToUpperInvariant();

        Guard.Condition(!normalized.Any(char.IsWhiteSpace), "Codigo da zona nao pode conter espacos.", "urban.zone.code");

        return normalized;
    }
}

public sealed class Restriction : AggregateRoot<RestrictionId>
{
    private Restriction(RestrictionId id, RestrictionType type, string description, Polygon boundary, Area area)
        : base(id)
    {
        Type = type;
        Description = Guard.NotNullOrWhiteSpace(description, nameof(description)).Trim();
        Boundary = Guard.NotNull(boundary, nameof(boundary));
        Area = Guard.NotNull(area, nameof(area));

        Guard.Condition(area.SquareMeters > 0, "Area da restricao deve ser maior que zero.", "urban.restriction.area");
    }

    public RestrictionType Type { get; }

    public string Description { get; private set; }

    public Polygon Boundary { get; }

    public Area Area { get; }

    public static Restriction Define(RestrictionType type, string description, Polygon boundary, Area area)
    {
        var restriction = new Restriction(RestrictionId.New(), type, description, boundary, area);
        restriction.Raise(new RestrictionDefined(restriction.Id, type));

        return restriction;
    }

    internal static Restriction Rehydrate(RestrictionId id, RestrictionType type, string description, Polygon boundary, Area area)
        => new(id, type, description, boundary, area);
}