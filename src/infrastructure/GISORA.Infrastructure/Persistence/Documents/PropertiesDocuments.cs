using GISORA.Domain.Geospatial;
using GISORA.Domain.Properties;

namespace GISORA.Infrastructure.Persistence.Documents;

internal sealed class BuildingDocument
{
    public Guid Id { get; init; }

    public BuildingType Type { get; init; }

    public decimal AreaSquareMeters { get; init; }

    public int Floors { get; init; }

    public Building ToDomain() => new(Id, Type, Area.FromSquareMeters(AreaSquareMeters), Floors);

    public static BuildingDocument FromDomain(Building building) => new()
    {
        Id = building.Id,
        Type = building.Type,
        AreaSquareMeters = building.Area.SquareMeters,
        Floors = building.Floors,
    };
}

internal sealed class UnitDocument
{
    public Guid Id { get; init; }

    public string Registration { get; init; } = string.Empty;

    public decimal AreaSquareMeters { get; init; }

    public UnitUsage Usage { get; init; }

    public Unit ToDomain() => new(Id, Registration, Area.FromSquareMeters(AreaSquareMeters), Usage);

    public static UnitDocument FromDomain(Unit unit) => new()
    {
        Id = unit.Id,
        Registration = unit.Registration,
        AreaSquareMeters = unit.Area.SquareMeters,
        Usage = unit.Usage,
    };
}

internal sealed class OwnershipDocument
{
    public Guid Id { get; init; }

    public Guid TaxpayerId { get; init; }

    public decimal Percentage { get; init; }

    public DateOnly Since { get; init; }

    public Ownership ToDomain() => new(Id, new TaxpayerId(TaxpayerId), OwnershipShare.FromPercentage(Percentage), Since);

    public static OwnershipDocument FromDomain(Ownership ownership) => new()
    {
        Id = ownership.Id,
        TaxpayerId = ownership.TaxpayerId.Value,
        Percentage = ownership.Share.Percentage,
        Since = ownership.Since,
    };
}

internal sealed class HistoryEntryDocument
{
    public Guid Id { get; init; }

    public DateTimeOffset ChangedAt { get; init; }

    public string Field { get; init; } = string.Empty;

    public string? PreviousValue { get; init; }

    public string? CurrentValue { get; init; }

    public CadastralHistoryEntry ToDomain() => new(Id, ChangedAt, Field, PreviousValue, CurrentValue);

    public static HistoryEntryDocument FromDomain(CadastralHistoryEntry entry) => new()
    {
        Id = entry.Id,
        ChangedAt = entry.ChangedAt,
        Field = entry.Field,
        PreviousValue = entry.PreviousValue,
        CurrentValue = entry.CurrentValue,
    };
}

internal sealed class PropertyDocument
{
    public Guid Id { get; init; }

    public string Sector { get; init; } = string.Empty;

    public string Block { get; init; } = string.Empty;

    public string Lot { get; init; } = string.Empty;

    public string? Unit { get; init; }

    public Guid StreetId { get; init; }

    public string Number { get; init; } = string.Empty;

    public Guid NeighborhoodId { get; init; }

    public string? Complement { get; init; }

    public decimal LandAreaSquareMeters { get; init; }

    public GeometryDocument? Geometry { get; init; }

    public List<BuildingDocument> Buildings { get; init; } = [];

    public List<UnitDocument> Units { get; init; } = [];

    public List<OwnershipDocument> Ownerships { get; init; } = [];

    public List<HistoryEntryDocument> History { get; init; } = [];

    public Property ToDomain() => Property.Rehydrate(
        new PropertyId(Id),
        PropertyRegistration.Create(Sector, Block, Lot, Unit),
        new Address(StreetId, Number, NeighborhoodId, Complement),
        Area.FromSquareMeters(LandAreaSquareMeters),
        Geometry is null ? null : (Polygon)Geometry.ToDomain(),
        Buildings.Select(building => building.ToDomain()),
        Units.Select(unit => unit.ToDomain()),
        Ownerships.Select(ownership => ownership.ToDomain()),
        History.Select(entry => entry.ToDomain()));

    public static PropertyDocument FromDomain(Property property) => new()
    {
        Id = property.Id.Value,
        Sector = property.Registration.Sector,
        Block = property.Registration.Block,
        Lot = property.Registration.Lot,
        Unit = property.Registration.Unit,
        StreetId = property.Address.StreetId,
        Number = property.Address.Number,
        NeighborhoodId = property.Address.NeighborhoodId,
        Complement = property.Address.Complement,
        LandAreaSquareMeters = property.LandArea.SquareMeters,
        Geometry = property.Geometry is null ? null : GeometryDocument.FromDomain(property.Geometry),
        Buildings = property.Buildings.Select(BuildingDocument.FromDomain).ToList(),
        Units = property.Units.Select(UnitDocument.FromDomain).ToList(),
        Ownerships = property.Ownerships.Select(OwnershipDocument.FromDomain).ToList(),
        History = property.History.Select(HistoryEntryDocument.FromDomain).ToList(),
    };
}

internal sealed class TaxpayerDocument
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Document { get; init; } = string.Empty;

    public Taxpayer ToDomain() => Taxpayer.Rehydrate(new TaxpayerId(Id), Name, DocumentNumber.Parse(Document));

    public static TaxpayerDocument FromDomain(Taxpayer taxpayer) => new()
    {
        Id = taxpayer.Id.Value,
        Name = taxpayer.Name,
        Document = taxpayer.Document.Digits,
    };
}

internal sealed record PropertyIndexEntry
{
    public Guid Id { get; init; }

    public string Registration { get; init; } = string.Empty;

    public string RegistrationSearch { get; init; } = string.Empty;

    public string Number { get; init; } = string.Empty;

    public Guid NeighborhoodId { get; init; }

    public decimal LandAreaSquareMeters { get; init; }

    public decimal BuiltAreaSquareMeters { get; init; }

    public int BuildingCount { get; init; }

    public static PropertyIndexEntry FromDomain(Property property) => new()
    {
        Id = property.Id.Value,
        Registration = property.Registration.Formatted,
        RegistrationSearch = property.Registration.Formatted.ToUpperInvariant(),
        Number = property.Address.Number,
        NeighborhoodId = property.Address.NeighborhoodId,
        LandAreaSquareMeters = property.LandArea.SquareMeters,
        BuiltAreaSquareMeters = property.BuiltArea.SquareMeters,
        BuildingCount = property.Buildings.Count,
    };
}