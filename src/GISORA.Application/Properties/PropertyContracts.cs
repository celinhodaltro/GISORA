using GISORA.Domain.Properties;

namespace GISORA.Application.Properties;

public sealed record CoordinateDto(double X, double Y);

public sealed record BuildingDto(Guid Id, BuildingType Type, decimal AreaSquareMeters, int Floors);

public sealed record UnitDto(Guid Id, string Registration, decimal AreaSquareMeters, UnitUsage Usage);

public sealed record OwnershipDto(Guid Id, TaxpayerId TaxpayerId, decimal Percentage, DateOnly Since);

public sealed record HistoryEntryDto(Guid Id, DateTimeOffset ChangedAt, string Field, string? PreviousValue, string? CurrentValue);

public sealed record PropertyDto(
    PropertyId Id,
    string Registration,
    Guid StreetId,
    string Number,
    Guid NeighborhoodId,
    decimal LandAreaSquareMeters,
    decimal BuiltAreaSquareMeters,
    decimal OwnershipTotal,
    IReadOnlyList<CoordinateDto> Geometry,
    IReadOnlyList<BuildingDto> Buildings,
    IReadOnlyList<UnitDto> Units,
    IReadOnlyList<OwnershipDto> Ownerships,
    IReadOnlyList<HistoryEntryDto> History);

public sealed record PropertyListItem(
    PropertyId Id,
    string Registration,
    string Number,
    decimal LandAreaSquareMeters,
    decimal BuiltAreaSquareMeters,
    int BuildingCount);

internal static class PropertyMapper
{
    public static PropertyDto ToDto(this Property property)
        => new(
            property.Id,
            property.Registration.Formatted,
            property.Address.StreetId,
            property.Address.Number,
            property.Address.NeighborhoodId,
            property.LandArea.SquareMeters,
            property.BuiltArea.SquareMeters,
            property.OwnershipTotal,
            property.Geometry is null
                ? []
                : property.Geometry.Coordinates
                    .Select(coordinate => new CoordinateDto(coordinate.X, coordinate.Y))
                    .ToArray(),
            property.Buildings
                .Select(building => new BuildingDto(building.Id, building.Type, building.Area.SquareMeters, building.Floors))
                .ToArray(),
            property.Units
                .Select(unit => new UnitDto(unit.Id, unit.Registration, unit.Area.SquareMeters, unit.Usage))
                .ToArray(),
            property.Ownerships
                .Select(ownership => new OwnershipDto(ownership.Id, ownership.TaxpayerId, ownership.Share.Percentage, ownership.Since))
                .ToArray(),
            property.History
                .OrderByDescending(entry => entry.ChangedAt)
                .Select(entry => new HistoryEntryDto(entry.Id, entry.ChangedAt, entry.Field, entry.PreviousValue, entry.CurrentValue))
                .ToArray());

    public static PropertyListItem ToListItem(this Property property)
        => new(
            property.Id,
            property.Registration.Formatted,
            property.Address.Number,
            property.LandArea.SquareMeters,
            property.BuiltArea.SquareMeters,
            property.Buildings.Count);
}