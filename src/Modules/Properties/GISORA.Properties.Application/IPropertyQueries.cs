using GISORA.BuildingBlocks.Application;

namespace GISORA.Properties.Application;

public sealed record PropertyFilter
{
    public string? RegistrationContains { get; init; }

    public Guid? NeighborhoodId { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 25;
}

public interface IPropertyQueries
{
    Task<PagedResult<PropertyListItem>> SearchAsync(PropertyFilter filter, CancellationToken cancellationToken = default);
}