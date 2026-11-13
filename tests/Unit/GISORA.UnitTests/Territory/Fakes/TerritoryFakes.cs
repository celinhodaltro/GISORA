using GISORA.Application.Common;
using GISORA.Application.Territory;
using GISORA.Domain.Territory;

namespace GISORA.UnitTests.Territory.Fakes;

internal sealed class InMemoryMunicipalityRepository : IMunicipalityRepository
{
    private readonly Dictionary<MunicipalityId, Municipality> _items = [];

    public int AddCount { get; private set; }

    public Task<Municipality?> FindAsync(MunicipalityId id, CancellationToken cancellationToken = default)
        => Task.FromResult(_items.GetValueOrDefault(id));

    public Task<bool> ExistsByIbgeCodeAsync(IbgeCode code, CancellationToken cancellationToken = default)
        => Task.FromResult(_items.Values.Any(municipality => municipality.IbgeCode == code));

    public Task AddAsync(Municipality municipality, CancellationToken cancellationToken = default)
    {
        _items.Add(municipality.Id, municipality);
        AddCount++;

        return Task.CompletedTask;
    }

    public Task UpdateAsync(Municipality municipality, CancellationToken cancellationToken = default)
    {
        _items[municipality.Id] = municipality;

        return Task.CompletedTask;
    }
}

internal sealed class InMemoryUrbanBlockRepository : IUrbanBlockRepository
{
    private readonly Dictionary<UrbanBlockId, UrbanBlock> _items = [];

    public Task<UrbanBlock?> FindAsync(UrbanBlockId id, CancellationToken cancellationToken = default)
        => Task.FromResult(_items.GetValueOrDefault(id));

    public Task<bool> ExistsByCodeAsync(FiscalSectorId fiscalSectorId, string code, CancellationToken cancellationToken = default)
        => Task.FromResult(_items.Values.Any(block =>
            block.FiscalSectorId == fiscalSectorId
            && string.Equals(block.Code, code.Trim(), StringComparison.OrdinalIgnoreCase)));

    public Task AddAsync(UrbanBlock urbanBlock, CancellationToken cancellationToken = default)
    {
        _items.Add(urbanBlock.Id, urbanBlock);

        return Task.CompletedTask;
    }

    public Task UpdateAsync(UrbanBlock urbanBlock, CancellationToken cancellationToken = default)
    {
        _items[urbanBlock.Id] = urbanBlock;

        return Task.CompletedTask;
    }
}

internal sealed class FakeUrbanBlockQueries : IUrbanBlockQueries
{
    public int Calls { get; private set; }

    public UrbanBlockFilter? LastFilter { get; private set; }

    public Task<PagedResult<UrbanBlockListItem>> SearchAsync(UrbanBlockFilter filter, CancellationToken cancellationToken = default)
    {
        Calls++;
        LastFilter = filter;

        return Task.FromResult(new PagedResult<UrbanBlockListItem>([], filter.PageNumber, filter.PageSize, 0));
    }
}