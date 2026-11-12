using GISORA.Roads.Application.Commands;
using GISORA.Roads.Domain;

namespace GISORA.Roads.UnitTests;

public class CreateStreetHandlerTests
{
    private sealed class InMemoryStreetRepository : IStreetRepository
    {
        private readonly Dictionary<StreetId, Street> _items = [];

        public Task<Street?> FindAsync(StreetId id, CancellationToken cancellationToken = default)
            => Task.FromResult(_items.GetValueOrDefault(id));

        public Task<bool> ExistsByNameAsync(string name, Guid? neighborhoodId, CancellationToken cancellationToken = default)
            => Task.FromResult(_items.Values.Any(street =>
                string.Equals(street.Name, name.Trim(), StringComparison.OrdinalIgnoreCase)
                && street.NeighborhoodId == neighborhoodId));

        public Task AddAsync(Street street, CancellationToken cancellationToken = default)
        {
            _items.Add(street.Id, street);

            return Task.CompletedTask;
        }

        public Task UpdateAsync(Street street, CancellationToken cancellationToken = default)
        {
            _items[street.Id] = street;

            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task CreatesStreet()
    {
        var handler = new CreateStreetHandler(new InMemoryStreetRepository());

        var result = await handler.HandleAsync(new CreateStreetCommand("Rua da Matriz", StreetType.Street, null));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task RejectsDuplicatedNameInSameNeighborhood()
    {
        var handler = new CreateStreetHandler(new InMemoryStreetRepository());
        await handler.HandleAsync(new CreateStreetCommand("Rua da Matriz", StreetType.Street, null));

        var result = await handler.HandleAsync(new CreateStreetCommand("rua da matriz", StreetType.Avenue, null));

        Assert.True(result.IsFailure);
        Assert.Equal("roads.street.duplicated", result.Error.Code);
    }

    [Fact]
    public async Task AllowsSameNameInDifferentNeighborhoods()
    {
        var repository = new InMemoryStreetRepository();
        var handler = new CreateStreetHandler(repository);
        await handler.HandleAsync(new CreateStreetCommand("Rua A", StreetType.Street, Guid.CreateVersion7()));

        var result = await handler.HandleAsync(new CreateStreetCommand("Rua A", StreetType.Street, Guid.CreateVersion7()));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task TranslatesDomainExceptionToValidationError()
    {
        var handler = new CreateStreetHandler(new InMemoryStreetRepository());

        var result = await handler.HandleAsync(new CreateStreetCommand(" ", StreetType.Street, null));

        Assert.True(result.IsFailure);
        Assert.Equal("guard.empty", result.Error.Code);
    }
}