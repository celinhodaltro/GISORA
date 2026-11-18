using GISORA.Application.Identity;
using GISORA.Application.Identity.Commands;
using GISORA.Domain.Identity;
using GISORA.Infrastructure.Identity;
using GISORA.UnitTests.Identity.Fakes;

namespace GISORA.UnitTests.Identity;

public class AuthenticationHandlerTests
{
    private readonly InMemoryUserRepository _users = new();
    private readonly FakePasswordHasher _hasher = new();
    private readonly JwtAccessTokenCodec _codec = new();
    private readonly InMemorySessionStore _session = new();

    [Fact]
    public async Task SignInWritesSessionAndRecordsAccess()
    {
        var user = await CreateUserAsync(UserRole.Analyst);
        var handler = new SignInHandler(_users, _hasher, _codec, _session);

        var result = await handler.HandleAsync(new SignInCommand("  MARIA@gisora.gov.br ", "Senha@2026"));

        Assert.True(result.IsSuccess);
        Assert.Equal(user.Id.Value, result.Value.Id);
        Assert.NotNull(_session.Token);
        Assert.NotNull((await _users.FindAsync(user.Id))!.LastSignInAt);
    }

    [Fact]
    public async Task SignInRejectsWrongPassword()
    {
        await CreateUserAsync(UserRole.Analyst);
        var handler = new SignInHandler(_users, _hasher, _codec, _session);

        var result = await handler.HandleAsync(new SignInCommand("maria@gisora.gov.br", "outra"));

        Assert.True(result.IsFailure);
        Assert.Equal("identity.invalid-credentials", result.Error.Code);
        Assert.Null(_session.Token);
    }

    [Fact]
    public async Task SignInRejectsInactiveUser()
    {
        var user = await CreateUserAsync(UserRole.Analyst);
        user.Deactivate();
        var handler = new SignInHandler(_users, _hasher, _codec, _session);

        var result = await handler.HandleAsync(new SignInCommand(user.Email, "Senha@2026"));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task SignInRejectsEmptyCredentials()
    {
        var handler = new SignInHandler(_users, _hasher, _codec, _session);

        var result = await handler.HandleAsync(new SignInCommand("  ", ""));

        Assert.True(result.IsFailure);
        Assert.Equal("identity.invalid-credentials", result.Error.Code);
    }

    [Fact]
    public async Task RestoreReturnsSessionForValidToken()
    {
        var user = await CreateUserAsync(UserRole.Analyst);
        _session.TokenValue = _codec.Issue(SessionOf(user), DateTimeOffset.UtcNow).Value;

        var result = await new RestoreSessionHandler(_users, _codec, _session).HandleAsync();

        Assert.NotNull(result);
        Assert.Equal(user.Id.Value, result.Id);
    }

    [Fact]
    public async Task RestoreClearsSessionForUnknownUser()
    {
        _session.TokenValue = _codec.Issue(
            new SessionUser(Guid.CreateVersion7(), "Fantasma", "fantasma@gisora.gov.br", UserRole.Analyst, []),
            DateTimeOffset.UtcNow).Value;

        var result = await new RestoreSessionHandler(_users, _codec, _session).HandleAsync();

        Assert.Null(result);
        Assert.Null(_session.TokenValue);
    }

    [Fact]
    public async Task RestoreClearsSessionForTamperedToken()
    {
        _session.TokenValue = "token.invalido.abc";

        var result = await new RestoreSessionHandler(_users, _codec, _session).HandleAsync();

        Assert.Null(result);
        Assert.Null(_session.TokenValue);
    }

    [Fact]
    public async Task SignOutClearsSession()
    {
        _session.TokenValue = "qualquer";

        await new SignOutHandler(_session).HandleAsync();

        Assert.Null(_session.TokenValue);
    }

    private async Task<User> CreateUserAsync(UserRole role)
    {
        var user = User.Create("Maria", "maria@gisora.gov.br", _hasher.Hash("Senha@2026"), role, DateTimeOffset.UtcNow);
        await _users.AddAsync(user);

        return user;
    }

    private static SessionUser SessionOf(User user)
        => new(user.Id.Value, user.Name, user.Email, user.Role, user.Permissions);

    private sealed class InMemorySessionStore : ISessionStore
    {
        public string? TokenValue { get; set; }

        public AccessToken? Token => TokenValue is null ? null : new AccessToken(TokenValue, DateTimeOffset.UtcNow.AddHours(8));

        public Task<string?> ReadAsync(CancellationToken cancellationToken = default) => Task.FromResult(TokenValue);

        public Task WriteAsync(AccessToken token, CancellationToken cancellationToken = default)
        {
            TokenValue = token.Value;

            return Task.CompletedTask;
        }

        public Task ClearAsync(CancellationToken cancellationToken = default)
        {
            TokenValue = null;

            return Task.CompletedTask;
        }
    }
}