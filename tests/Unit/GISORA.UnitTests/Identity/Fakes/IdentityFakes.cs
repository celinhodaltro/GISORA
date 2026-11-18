using GISORA.Domain.Identity;

namespace GISORA.UnitTests.Identity.Fakes;

internal sealed class FakePasswordHasher : IPasswordHasher
{
    public HashedPassword Hash(string password) => new($"hash::{password}", "salt");

    public bool Verify(string password, HashedPassword hashedPassword) => hashedPassword.Hash == $"hash::{password}";
}

internal sealed class InMemoryUserRepository : IUserRepository
{
    private readonly Dictionary<UserId, User> _items = [];

    public Task<User?> FindAsync(UserId id, CancellationToken cancellationToken = default)
        => Task.FromResult(_items.GetValueOrDefault(id));

    public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();

        return Task.FromResult(_items.Values.FirstOrDefault(user => user.Email == normalized));
    }

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();

        return Task.FromResult(_items.Values.Any(user => user.Email == normalized));
    }

    public Task<int> CountActiveAdministratorsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_items.Values.Count(user => user.IsActive && user.Role == UserRole.Administrator));

    public Task<IReadOnlyList<User>> ListAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<User>>(_items.Values.ToArray());

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _items.Add(user.Id, user);

        return Task.CompletedTask;
    }

    public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _items[user.Id] = user;

        return Task.CompletedTask;
    }
}