using GISORA.Domain.Common;
using GISORA.Domain.Geospatial;

namespace GISORA.Domain.Rural;

public enum RuralUsage
{
    Agriculture,
    Livestock,
    Mixed,
    Forestry,
    Other,
}

public enum RuralSourceKind
{
    Manual,
    GeoJson,
}

public sealed record RuralPropertyId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static RuralPropertyId New() => new(Guid.CreateVersion7());
}

public sealed record PreservationAreaId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static PreservationAreaId New() => new(Guid.CreateVersion7());
}

public sealed record RuralPropertyRegistered(RuralPropertyId PropertyId, string Name) : DomainEvent;

public sealed record LegalReserveDefined(RuralPropertyId PropertyId, decimal AreaSquareMeters) : DomainEvent;

public sealed record PreservationAreaAdded(RuralPropertyId PropertyId, PreservationAreaId AreaId) : DomainEvent;

public sealed record PreservationAreaRemoved(RuralPropertyId PropertyId, PreservationAreaId AreaId) : DomainEvent;

public sealed record RuralSource(string Kind, string Crs, DateOnly ImportedAt)
{
    public static RuralSource Manual { get; } = new(nameof(RuralSourceKind.Manual), "EPSG:4326", DateOnly.FromDateTime(DateTime.UtcNow));

    public static RuralSource FromGeoJson(string crs) => new(nameof(RuralSourceKind.GeoJson), crs, DateOnly.FromDateTime(DateTime.UtcNow));
}

public sealed class LegalReserve
{
    private LegalReserve(Polygon boundary, Area area)
    {
        Boundary = Guard.NotNull(boundary, nameof(boundary));
        Area = Guard.NotNull(area, nameof(area));

        Guard.Condition(area.SquareMeters > 0, "Reserva legal deve ter area maior que zero.", "rural.legal-reserve.area");
    }

    public Polygon Boundary { get; }

    public Area Area { get; }

    public static LegalReserve Define(Polygon boundary, Area area) => new(boundary, area);
}

public sealed class PreservationArea
{
    private PreservationArea(PreservationAreaId id, string label, Polygon boundary, Area area)
    {
        Id = Guard.NotNull(id, nameof(id));
        Label = Guard.NotNullOrWhiteSpace(label, nameof(label)).Trim();
        Boundary = Guard.NotNull(boundary, nameof(boundary));
        Area = Guard.NotNull(area, nameof(area));

        Guard.Condition(area.SquareMeters > 0, "APP deve ter area maior que zero.", "rural.app.area");
    }

    public PreservationAreaId Id { get; }

    public string Label { get; }

    public Polygon Boundary { get; }

    public Area Area { get; }

    public static PreservationArea Create(string label, Polygon boundary, Area area)
        => new(PreservationAreaId.New(), label, boundary, area);

    internal static PreservationArea Rehydrate(PreservationAreaId id, string label, Polygon boundary, Area area)
        => new(id, label, boundary, area);
}

public sealed class RuralProperty : AggregateRoot<RuralPropertyId>
{
    private readonly List<PreservationArea> _preservationAreas = [];

    private RuralProperty(
        RuralPropertyId id,
        string name,
        Polygon boundary,
        Area area,
        RuralUsage usage,
        Guid? municipalityId,
        string? carCode,
        string? sigefCode,
        string? snciCode,
        RuralSource source,
        LegalReserve? legalReserve,
        IEnumerable<PreservationArea> preservationAreas)
        : base(id)
    {
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name)).Trim();
        Boundary = Guard.NotNull(boundary, nameof(boundary));
        Area = Guard.NotNull(area, nameof(area));
        Usage = usage;
        MunicipalityId = municipalityId;
        CarCode = NormalizeCode(carCode, "CAR");
        SigefCode = NormalizeCode(sigefCode, "SIGEF");
        SnciCode = NormalizeCode(snciCode, "SNCI");
        Source = Guard.NotNull(source, nameof(source));
        LegalReserve = legalReserve;

        Guard.Condition(area.SquareMeters > 0, "Area da propriedade deve ser maior que zero.", "rural.property.area");
        Guard.Condition(
            legalReserve is null || legalReserve.Area.SquareMeters <= area.SquareMeters,
            "Reserva legal nao pode exceder a area da propriedade.",
            "rural.legal-reserve.exceeds-property");

        _preservationAreas.AddRange(preservationAreas);

        Guard.Condition(
            ConstrainedSquareMeters <= area.SquareMeters,
            "Soma de reserva legal e APP nao pode exceder a area da propriedade.",
            "rural.constraints.exceeds-property");
    }

    public string Name { get; private set; }

    public Polygon Boundary { get; private set; }

    public Area Area { get; private set; }

    public RuralUsage Usage { get; private set; }

    public Guid? MunicipalityId { get; private set; }

    public string? CarCode { get; private set; }

    public string? SigefCode { get; private set; }

    public string? SnciCode { get; private set; }

    public RuralSource Source { get; private set; }

    public LegalReserve? LegalReserve { get; private set; }

    public IReadOnlyList<PreservationArea> PreservationAreas => _preservationAreas.AsReadOnly();

    public decimal ConstrainedSquareMeters
        => (LegalReserve?.Area.SquareMeters ?? 0m) + _preservationAreas.Sum(area => area.Area.SquareMeters);

    public static RuralProperty Register(
        string name,
        Polygon boundary,
        Area area,
        RuralUsage usage,
        Guid? municipalityId = null,
        string? carCode = null,
        string? sigefCode = null,
        string? snciCode = null,
        RuralSource? source = null)
    {
        var property = new RuralProperty(
            RuralPropertyId.New(),
            name,
            boundary,
            area,
            usage,
            municipalityId,
            carCode,
            sigefCode,
            snciCode,
            source ?? RuralSource.Manual,
            null,
            []);

        property.Raise(new RuralPropertyRegistered(property.Id, property.Name));

        return property;
    }

    public void Rename(string name) => Name = Guard.NotNullOrWhiteSpace(name, nameof(name)).Trim();

    public void ChangeUsage(RuralUsage usage) => Usage = usage;

    public void UpdateCodes(string? carCode, string? sigefCode, string? snciCode)
    {
        CarCode = NormalizeCode(carCode, "CAR");
        SigefCode = NormalizeCode(sigefCode, "SIGEF");
        SnciCode = NormalizeCode(snciCode, "SNCI");
    }

    public void DefineLegalReserve(Polygon boundary, Area area)
    {
        Guard.NotNull(boundary, nameof(boundary));
        Guard.NotNull(area, nameof(area));

        Guard.Condition(area.SquareMeters > 0, "Reserva legal deve ter area maior que zero.", "rural.legal-reserve.area");
        Guard.Condition(
            area.SquareMeters <= Area.SquareMeters,
            "Reserva legal nao pode exceder a area da propriedade.",
            "rural.legal-reserve.exceeds-property");
        Guard.Condition(
            area.SquareMeters + _preservationAreas.Sum(item => item.Area.SquareMeters) <= Area.SquareMeters,
            "Soma de reserva legal e APP nao pode exceder a area da propriedade.",
            "rural.constraints.exceeds-property");

        LegalReserve = LegalReserve.Define(boundary, area);
        Raise(new LegalReserveDefined(Id, area.SquareMeters));
    }

    public void ClearLegalReserve() => LegalReserve = null;

    public void AddPreservationArea(PreservationArea area)
    {
        Guard.NotNull(area, nameof(area));
        Guard.Condition(
            area.Area.SquareMeters <= Area.SquareMeters,
            "APP nao pode exceder a area da propriedade.",
            "rural.app.exceeds-property");
        Guard.Condition(
            ConstrainedSquareMeters + area.Area.SquareMeters <= Area.SquareMeters,
            "Soma de reserva legal e APP nao pode exceder a area da propriedade.",
            "rural.constraints.exceeds-property");

        _preservationAreas.Add(area);
        Raise(new PreservationAreaAdded(Id, area.Id));
    }

    public void RemovePreservationArea(PreservationAreaId areaId)
    {
        var area = _preservationAreas.FirstOrDefault(item => item.Id == areaId);

        if (area is null)
        {
            return;
        }

        _preservationAreas.Remove(area);
        Raise(new PreservationAreaRemoved(Id, areaId));
    }

    internal static RuralProperty Rehydrate(
        RuralPropertyId id,
        string name,
        Polygon boundary,
        Area area,
        RuralUsage usage,
        Guid? municipalityId,
        string? carCode,
        string? sigefCode,
        string? snciCode,
        RuralSource source,
        LegalReserve? legalReserve,
        IEnumerable<PreservationArea> preservationAreas)
        => new(
            id,
            name,
            boundary,
            area,
            usage,
            municipalityId,
            carCode,
            sigefCode,
            snciCode,
            source,
            legalReserve,
            preservationAreas);

    private static string? NormalizeCode(string? code, string field)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        var normalized = code.Trim().ToUpperInvariant();

        Guard.Condition(normalized.Length <= 80, $"Codigo {field} deve ter no maximo 80 caracteres.", "rural.code.length");
        Guard.Condition(!normalized.Any(char.IsWhiteSpace), $"Codigo {field} nao pode conter espacos.", "rural.code.whitespace");

        return normalized;
    }
}