using GISORA.BuildingBlocks.Application;
using GISORA.Roads.Domain;

namespace GISORA.Roads.Application;

public sealed record StreetListItem(StreetId Id, string Name, StreetType Type, Guid? NeighborhoodId);

public sealed record StreetFilter
{
    public string? NameContains { get; init; }

    public StreetType? Type { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 25;
}

public interface IStreetQueries
{
    Task<PagedResult<StreetListItem>> SearchAsync(StreetFilter filter, CancellationToken cancellationToken = default);
}