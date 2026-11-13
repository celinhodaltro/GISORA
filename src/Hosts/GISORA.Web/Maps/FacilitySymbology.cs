using GISORA.PublicFacilities.Domain;

namespace GISORA.Web.Maps;

public sealed record FacilitySymbol(FacilityType Type, string Label, string Icon, string Color);

public static class FacilitySymbology
{
    public static IReadOnlyList<FacilitySymbol> All { get; } =
    [
        new(FacilityType.School, "Escola", "school", "#1d4ed8"),
        new(FacilityType.HealthCenter, "Unidade de saude", "medical_services", "#0f766e"),
        new(FacilityType.Hospital, "Hospital", "local_hospital", "#b91c1c"),
        new(FacilityType.Square, "Praca", "local_florist", "#a16207"),
        new(FacilityType.Park, "Parque", "park", "#15803d"),
        new(FacilityType.PublicBuilding, "Predio publico", "location_city", "#5b21b6"),
        new(FacilityType.Other, "Outro", "place", "#475569"),
    ];

    public static FacilitySymbol For(FacilityType type)
        => All.First(symbol => symbol.Type == type);
}