using GISORA.BuildingBlocks.Application;
using GISORA.Territory.Domain;

namespace GISORA.Territory.Application;

public sealed record UrbanBlockFilter
{
    public FiscalSectorId? FiscalSectorId { get; init; }

    public string? CodeContains { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 25;
}

public interface IUrbanBlockQueries
{
    Task<PagedResult<UrbanBlockListItem>> SearchAsync(UrbanBlockFilter filter, CancellationToken cancellationToken = default);
}