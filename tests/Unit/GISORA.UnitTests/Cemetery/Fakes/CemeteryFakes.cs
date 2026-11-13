using GISORA.Application.Geospatial;
using GISORA.Domain.Cemetery;
using GISORA.Domain.Geospatial;

using CemeteryAggregate = GISORA.Domain.Cemetery.Cemetery;

namespace GISORA.UnitTests.Cemetery.Fakes;

internal sealed class InMemoryCemeteryRepository : ICemeteryRepository
{
    public Dictionary<CemeteryId, CemeteryAggregate> Items { get; } = [];

    public Task<CemeteryAggregate?> FindAsync(CemeteryId id, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.GetValueOrDefault(id));

    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.Values.Any(cemetery =>
            string.Equals(cemetery.Name, name.Trim(), StringComparison.OrdinalIgnoreCase)));

    public Task AddAsync(CemeteryAggregate cemetery, CancellationToken cancellationToken = default)
    {
        Items.Add(cemetery.Id, cemetery);

        return Task.CompletedTask;
    }

    public Task UpdateAsync(CemeteryAggregate cemetery, CancellationToken cancellationToken = default)
    {
        Items[cemetery.Id] = cemetery;

        return Task.CompletedTask;
    }
}

internal sealed class InMemoryCemeterySectorRepository : ICemeterySectorRepository
{
    public Dictionary<CemeterySectorId, CemeterySector> Items { get; } = [];

    public Task<CemeterySector?> FindAsync(CemeterySectorId id, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.GetValueOrDefault(id));

    public Task<bool> ExistsByCodeAsync(CemeteryId cemeteryId, string code, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.Values.Any(sector =>
            sector.CemeteryId == cemeteryId
            && string.Equals(sector.Code, code.Trim(), StringComparison.OrdinalIgnoreCase)));

    public Task AddAsync(CemeterySector sector, CancellationToken cancellationToken = default)
    {
        Items.Add(sector.Id, sector);

        return Task.CompletedTask;
    }

    public Task UpdateAsync(CemeterySector sector, CancellationToken cancellationToken = default)
    {
        Items[sector.Id] = sector;

        return Task.CompletedTask;
    }
}

internal sealed class InMemoryGraveRepository : IGraveRepository
{
    public Dictionary<GraveId, Grave> Items { get; } = [];

    public Task<Grave?> FindAsync(GraveId id, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.GetValueOrDefault(id));

    public Task<bool> ExistsByCodeAsync(CemeterySectorId sectorId, string code, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.Values.Any(grave =>
            grave.SectorId == sectorId
            && string.Equals(grave.Code, code.Trim(), StringComparison.OrdinalIgnoreCase)));

    public Task AddAsync(Grave grave, CancellationToken cancellationToken = default)
    {
        Items.Add(grave.Id, grave);

        return Task.CompletedTask;
    }

    public Task UpdateAsync(Grave grave, CancellationToken cancellationToken = default)
    {
        Items[grave.Id] = grave;

        return Task.CompletedTask;
    }
}

internal sealed class FakeSpatialOperations : ISpatialOperations
{
    public bool ContainsResult { get; set; } = true;

    public decimal AreaSquareMeters { get; set; } = 1_000m;

    public Task<Area> AreaAsync(Geometry geometry, CancellationToken cancellationToken = default)
        => Task.FromResult(Area.FromSquareMeters(AreaSquareMeters));

    public Task<bool> ContainsAsync(Geometry container, Geometry contained, CancellationToken cancellationToken = default)
        => Task.FromResult(ContainsResult);

    public Task<Distance> DistanceAsync(Geometry origin, Geometry target, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public Task<bool> IntersectsAsync(Geometry left, Geometry right, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public Task<Geometry?> IntersectionAsync(Geometry left, Geometry right, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();
}