using GISORA.Domain.Common;
using GISORA.Domain.Geospatial;

namespace GISORA.Domain.Properties;

public sealed record PropertyRegistered(PropertyId PropertyId, string Registration) : DomainEvent;

public sealed record PropertyGeometryChanged(PropertyId PropertyId) : DomainEvent;

public sealed record OwnershipChanged(PropertyId PropertyId) : DomainEvent;

public sealed class Property : AggregateRoot<PropertyId>
{
    private const decimal OwnershipTolerance = 0.01m;

    private readonly List<Building> _buildings = [];
    private readonly List<Unit> _units = [];
    private readonly List<Ownership> _ownerships = [];
    private readonly List<CadastralHistoryEntry> _history = [];

    private Property(PropertyId id, PropertyRegistration registration, Address address, Area landArea, Polygon? geometry)
        : base(id)
    {
        Guard.Condition(landArea.SquareMeters > 0, "Area do lote deve ser maior que zero.", "properties.land-area");

        Registration = Guard.NotNull(registration, nameof(registration));
        Address = Guard.NotNull(address, nameof(address));
        LandArea = landArea;
        Geometry = geometry;
    }

    public PropertyRegistration Registration { get; }

    public Address Address { get; private set; }

    public Area LandArea { get; }

    public Polygon? Geometry { get; private set; }

    public IReadOnlyCollection<Building> Buildings => _buildings.AsReadOnly();

    public IReadOnlyCollection<Unit> Units => _units.AsReadOnly();

    public IReadOnlyCollection<Ownership> Ownerships => _ownerships.AsReadOnly();

    public IReadOnlyCollection<CadastralHistoryEntry> History => _history.AsReadOnly();

    public Area BuiltArea => Area.FromSquareMeters(_buildings.Sum(building => building.Area.SquareMeters));

    public decimal OwnershipTotal => _ownerships.Sum(ownership => ownership.Share.Percentage);

    public static Property Register(
        PropertyRegistration registration,
        Address address,
        Area landArea,
        Polygon? geometry = null,
        DateTimeOffset? registeredAt = null)
    {
        var property = new Property(PropertyId.New(), registration, address, landArea, geometry);

        property.Raise(new PropertyRegistered(property.Id, registration.Formatted)
        {
            OccurredOn = (registeredAt ?? DateTimeOffset.UtcNow).ToUniversalTime(),
        });

        return property;
    }

    public void ChangeGeometry(Polygon geometry, string? reason, DateTimeOffset? changedAt = null)
    {
        Guard.NotNull(geometry, nameof(geometry));

        if (Geometry == geometry)
        {
            return;
        }

        _history.Add(new CadastralHistoryEntry(
            Guid.CreateVersion7(),
            changedAt ?? DateTimeOffset.UtcNow,
            "geometry",
            Describe(Geometry),
            Describe(geometry)));

        Geometry = geometry;
        Raise(new PropertyGeometryChanged(Id));
    }

    public void ChangeAddress(Address address, DateTimeOffset? changedAt = null)
    {
        Guard.NotNull(address, nameof(address));

        if (Address == address)
        {
            return;
        }

        _history.Add(new CadastralHistoryEntry(
            Guid.CreateVersion7(),
            changedAt ?? DateTimeOffset.UtcNow,
            "address",
            Address.ToString(),
            address.ToString()));

        Address = address;
    }

    public Building AddBuilding(BuildingType type, Area area, int floors)
    {
        var newTotal = BuiltArea.SquareMeters + area.SquareMeters;

        Guard.Condition(
            newTotal <= LandArea.SquareMeters,
            "Area construida nao pode exceder a area do lote.",
            "properties.building.exceeds-land-area");

        var building = new Building(Guid.CreateVersion7(), type, area, floors);
        _buildings.Add(building);

        return building;
    }

    public Unit AddUnit(string registration, Area area, UnitUsage usage)
    {
        var unit = new Unit(Guid.CreateVersion7(), registration, area, usage);
        _units.Add(unit);

        return unit;
    }

    public Ownership AddOwnership(TaxpayerId taxpayerId, OwnershipShare share, DateOnly since)
    {
        var newTotal = OwnershipTotal + share.Percentage;

        Guard.Condition(
            newTotal <= 100m + OwnershipTolerance,
            "Soma das titularidades nao pode exceder 100%.",
            "properties.ownership.exceeds-total");

        var ownership = new Ownership(Guid.CreateVersion7(), taxpayerId, share, since);
        _ownerships.Add(ownership);
        Raise(new OwnershipChanged(Id));

        return ownership;
    }

    public void EnsureOwnershipBalanced()
    {
        if (_ownerships.Count == 0)
        {
            return;
        }

        Guard.Condition(
            Math.Abs(OwnershipTotal - 100m) <= OwnershipTolerance,
            $"Soma das titularidades deve ser 100%. Atual: {OwnershipTotal}%.",
            "properties.ownership.unbalanced");
    }

    internal static Property Rehydrate(
        PropertyId id,
        PropertyRegistration registration,
        Address address,
        Area landArea,
        Polygon? geometry,
        IEnumerable<Building> buildings,
        IEnumerable<Unit> units,
        IEnumerable<Ownership> ownerships,
        IEnumerable<CadastralHistoryEntry> history)
    {
        var property = new Property(id, registration, address, landArea, geometry);

        property._buildings.AddRange(buildings);
        property._units.AddRange(units);
        property._ownerships.AddRange(ownerships);
        property._history.AddRange(history);

        return property;
    }

    private static string? Describe(Geometry? geometry)
        => geometry is null ? null : $"{geometry.Type}:{geometry.Coordinates.Count}";
}