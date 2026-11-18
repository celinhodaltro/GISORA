using GISORA.Domain.Identity;
using GISORA.Infrastructure.Persistence.Repositories;
using GISORA.UnitTests.Infrastructure.LocalStorage.Fakes;

namespace GISORA.UnitTests.Identity;

public class UserRepositoryTests
{
    private readonly InMemoryLocalStore _store = new();
    private readonly LocalStorageUserRepository _repository;

    public UserRepositoryTests() => _repository = new LocalStorageUserRepository(_store);

    [Fact]
    public async Task RoundTripsUserWithHashAndRole()
    {
        var user = Create("Maria", "maria@gisora.gov.br", UserRole.Analyst);

        await _repository.AddAsync(user);
        var restored = await _repository.FindAsync(user.Id);

        Assert.NotNull(restored);
        Assert.Equal(user.Email, restored.Email);
        Assert.Equal(user.PasswordHash, restored.PasswordHash);
        Assert.Equal(user.PasswordSalt, restored.PasswordSalt);
        Assert.Equal(UserRole.Analyst, restored.Role);
        Assert.True(restored.IsActive);
    }

    [Fact]
    public async Task FindsUserByEmailIgnoringCase()
    {
        await _repository.AddAsync(Create("Maria", "maria@gisora.gov.br", UserRole.Analyst));

        var found = await _repository.FindByEmailAsync("  MARIA@gisora.gov.br ");

        Assert.NotNull(found);
    }

    [Fact]
    public async Task RejectsDuplicatedEmail()
    {
        await _repository.AddAsync(Create("Maria", "maria@gisora.gov.br", UserRole.Analyst));

        await Assert.ThrowsAsync<GISORA.Infrastructure.Persistence.StorageException>(() =>
            _repository.AddAsync(Create("Outra", "MARIA@gisora.gov.br", UserRole.Analyst)));
    }

    [Fact]
    public async Task CountsOnlyActiveAdministrators()
    {
        var active = Create("Admin", "admin@gisora.gov.br", UserRole.Administrator);
        var inactive = Create("Admin 2", "admin2@gisora.gov.br", UserRole.Administrator);
        inactive.Deactivate();
        await _repository.AddAsync(active);
        await _repository.AddAsync(inactive);
        await _repository.AddAsync(Create("Analista", "analista@gisora.gov.br", UserRole.Analyst));

        var total = await _repository.CountActiveAdministratorsAsync();

        Assert.Equal(1, total);
    }

    [Fact]
    public async Task UpdatePersistsChangedRole()
    {
        var user = Create("Maria", "maria@gisora.gov.br", UserRole.Analyst);
        await _repository.AddAsync(user);

        user.ChangeRole(UserRole.Administrator);
        await _repository.UpdateAsync(user);

        Assert.Equal(UserRole.Administrator, (await _repository.FindAsync(user.Id))!.Role);
    }

    [Fact]
    public async Task ListsAllStoredUsers()
    {
        await _repository.AddAsync(Create("Maria", "maria@gisora.gov.br", UserRole.Analyst));
        await _repository.AddAsync(Create("Joao", "joao@gisora.gov.br", UserRole.PublicViewer));

        var listed = await _repository.ListAsync();

        Assert.Equal(2, listed.Count);
    }

    private static User Create(string name, string email, UserRole role)
        => User.Create(name, email, new HashedPassword($"hash::{email}", "salt"), role, DateTimeOffset.UtcNow);
}