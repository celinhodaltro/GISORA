using GISORA.Geospatial.Domain;
using GISORA.PublicFacilities.Application.Commands;
using GISORA.PublicFacilities.Domain;

namespace GISORA.PublicFacilities.UnitTests;

public class CreateFacilityHandlerTests
{
    private sealed class InMemoryFacilityRepository : IPublicFacilityRepository
    {
        private readonly Dictionary<PublicFacilityId, PublicFacility> _items = [];

        public Task<PublicFacility?> FindAsync(PublicFacilityId id, CancellationToken cancellationToken = default)
            => Task.FromResult(_items.GetValueOrDefault(id));

        public Task<bool> ExistsByNameAsync(string name, Guid? neighborhoodId, CancellationToken cancellationToken = default)
            => Task.FromResult(_items.Values.Any(facility =>
                string.Equals(facility.Name, name.Trim(), StringComparison.OrdinalIgnoreCase)
                && facility.NeighborhoodId == neighborhoodId));

        public Task AddAsync(PublicFacility facility, CancellationToken cancellationToken = default)
        {
            _items.Add(facility.Id, facility);

            return Task.CompletedTask;
        }

        public Task UpdateAsync(PublicFacility facility, CancellationToken cancellationToken = default)
        {
            _items[facility.Id] = facility;

            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task CreatesFacilityWithLocation()
    {
        var handler = new CreateFacilityHandler(new InMemoryFacilityRepository());
        var point = new Point(new Coordinate(-35.25, -7.95, SpatialReference.Wgs84));

        var result = await handler.HandleAsync(new CreateFacilityCommand("Escola Municipal", FacilityType.School, null, point));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task RejectsDuplicatedNameInSameNeighborhood()
    {
        var handler = new CreateFacilityHandler(new InMemoryFacilityRepository());
        await handler.HandleAsync(new CreateFacilityCommand("Escola Municipal", FacilityType.School, null, null));

        var result = await handler.HandleAsync(new CreateFacilityCommand("escola municipal", FacilityType.Other, null, null));

        Assert.True(result.IsFailure);
        Assert.Equal("facilities.duplicated", result.Error.Code);
    }

    [Fact]
    public async Task AllowsSameNameInDifferentNeighborhoods()
    {
        var handler = new CreateFacilityHandler(new InMemoryFacilityRepository());
        await handler.HandleAsync(new CreateFacilityCommand("Escola Municipal", FacilityType.School, Guid.CreateVersion7(), null));

        var result = await handler.HandleAsync(new CreateFacilityCommand("Escola Municipal", FacilityType.School, Guid.CreateVersion7(), null));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task TranslatesDomainExceptionToValidationError()
    {
        var handler = new CreateFacilityHandler(new InMemoryFacilityRepository());

        var result = await handler.HandleAsync(new CreateFacilityCommand(" ", FacilityType.School, null, null));

        Assert.True(result.IsFailure);
        Assert.Equal("guard.empty", result.Error.Code);
    }
}