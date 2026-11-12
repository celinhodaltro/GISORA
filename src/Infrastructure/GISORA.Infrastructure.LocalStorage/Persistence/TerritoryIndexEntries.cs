namespace GISORA.Infrastructure.LocalStorage.Persistence;

internal sealed record DistrictIndexEntry
{
    public Guid Id { get; init; }

    public Guid MunicipalityId { get; init; }

    public string Name { get; init; } = string.Empty;

    public string NameSearch { get; init; } = string.Empty;
}

internal sealed record NeighborhoodIndexEntry
{
    public Guid Id { get; init; }

    public Guid DistrictId { get; init; }

    public string Name { get; init; } = string.Empty;

    public string NameSearch { get; init; } = string.Empty;
}

internal sealed record FiscalSectorIndexEntry
{
    public Guid Id { get; init; }

    public Guid DistrictId { get; init; }

    public string Code { get; init; } = string.Empty;

    public string CodeSearch { get; init; } = string.Empty;
}