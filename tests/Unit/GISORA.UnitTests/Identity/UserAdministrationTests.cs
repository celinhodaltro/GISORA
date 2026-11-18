using GISORA.Application.Identity.Commands;
using GISORA.Domain.Identity;
using GISORA.UnitTests.Identity.Fakes;

namespace GISORA.UnitTests.Identity;

public class UserAdministrationTests
{
    private readonly InMemoryUserRepository _users = new();
    private readonly FakePasswordHasher _hasher = new();

    [Fact]
    public async Task RegisterUserStoresHashedPassword()
    {
        var handler = new RegisterUserHandler(_users, _hasher);

        var result = await handler.HandleAsync(new RegisterUserCommand("Maria", "Maria@GISORA.gov.br", "Senha@2026", UserRole.Analyst));

        Assert.True(result.IsSuccess);
        var user = await _users.FindByEmailAsync("maria@gisora.gov.br");
        Assert.NotNull(user);
        Assert.Equal("hash::Senha@2026", user.PasswordHash);
        Assert.NotEqual("Senha@2026", user.PasswordHash);
    }

    [Fact]
    public async Task RegisterUserRejectsDuplicatedEmail()
    {
        var handler = new RegisterUserHandler(_users, _hasher);
        await handler.HandleAsync(new RegisterUserCommand("Maria", "maria@gisora.gov.br", "Senha@2026", UserRole.Analyst));

        var result = await handler.HandleAsync(new RegisterUserCommand("Outra", "MARIA@gisora.gov.br", "Senha@2026", UserRole.Analyst));

        Assert.True(result.IsFailure);
        Assert.Equal("identity.email-duplicated", result.Error.Code);
    }

    [Fact]
    public async Task RegisterUserRejectsShortPassword()
    {
        var handler = new RegisterUserHandler(_users, _hasher);

        var result = await handler.HandleAsync(new RegisterUserCommand("Maria", "maria@gisora.gov.br", "123", UserRole.Analyst));

        Assert.True(result.IsFailure);
        Assert.Equal("identity.password-length", result.Error.Code);
    }

    [Fact]
    public async Task UpdateUserBlocksDeactivatingLastAdministrator()
    {
        var administrator = await CreateAsync("Admin", "admin@gisora.gov.br", UserRole.Administrator);
        var handler = new UpdateUserHandler(_users);

        var result = await handler.HandleAsync(new UpdateUserCommand(administrator.Id.Value, UserRole.Administrator, false));

        Assert.True(result.IsFailure);
        Assert.Equal("identity.last-administrator", result.Error.Code);
    }

    [Fact]
    public async Task UpdateUserChangesRoleWhenAnotherAdministratorExists()
    {
        var administrator = await CreateAsync("Admin", "admin@gisora.gov.br", UserRole.Administrator);
        await CreateAsync("Admin 2", "admin2@gisora.gov.br", UserRole.Administrator);
        var handler = new UpdateUserHandler(_users);

        var result = await handler.HandleAsync(new UpdateUserCommand(administrator.Id.Value, UserRole.Analyst, true));

        Assert.True(result.IsSuccess);
        Assert.Equal(UserRole.Analyst, (await _users.FindAsync(administrator.Id))!.Role);
    }

    [Fact]
    public async Task ResetPasswordReplacesSecret()
    {
        var user = await CreateAsync("Maria", "maria@gisora.gov.br", UserRole.Analyst);

        var result = await new ResetUserPasswordHandler(_users, _hasher)
            .HandleAsync(new ResetUserPasswordCommand(user.Id.Value, "NovaSenha@2026"));

        Assert.True(result.IsSuccess);
        Assert.Equal("hash::NovaSenha@2026", (await _users.FindAsync(user.Id))!.PasswordHash);
    }

    [Fact]
    public async Task EnsureIdentityUsersSeedsThreeAccountsOnlyOnce()
    {
        var handler = new EnsureIdentityUsersHandler(_users, _hasher);

        var created = await handler.HandleAsync();
        var again = await handler.HandleAsync();

        Assert.Equal(3, created);
        Assert.Equal(0, again);
        Assert.NotNull(await _users.FindByEmailAsync("admin@gisora.gov.br"));
        Assert.NotNull(await _users.FindByEmailAsync("analista@gisora.gov.br"));
        Assert.NotNull(await _users.FindByEmailAsync("consulta@gisora.gov.br"));
    }

    private async Task<User> CreateAsync(string name, string email, UserRole role)
    {
        var user = User.Create(name, email, _hasher.Hash("Senha@2026"), role, DateTimeOffset.UtcNow);
        await _users.AddAsync(user);

        return user;
    }
}