using GISORA.Application.Common;
using GISORA.Domain.Identity;

namespace GISORA.Application.Identity.Commands;

public sealed record SignInCommand(string Email, string Password);

public sealed class SignInHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IAccessTokenCodec accessTokenCodec,
    ISessionStore sessionStore)
{
    public async Task<Result<SessionUser>> HandleAsync(SignInCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Email) || string.IsNullOrEmpty(command.Password))
        {
            return InvalidCredentials();
        }

        var email = command.Email.Trim().ToLowerInvariant();
        var user = await users.FindByEmailAsync(email, cancellationToken);

        if (user is null || user.IsActive is false)
        {
            return InvalidCredentials();
        }

        if (passwordHasher.Verify(command.Password, new HashedPassword(user.PasswordHash, user.PasswordSalt)) is false)
        {
            return InvalidCredentials();
        }

        var now = DateTimeOffset.UtcNow;
        user.RecordSignIn(now);
        await users.UpdateAsync(user, cancellationToken);

        var sessionUser = SessionUserMapper.FromDomain(user);
        await sessionStore.WriteAsync(accessTokenCodec.Issue(sessionUser, now), cancellationToken);

        return Result.Ok(sessionUser);
    }

    private static Result<SessionUser> InvalidCredentials()
        => Result.Fail<SessionUser>(Error.Unauthorized("E-mail ou senha invalidos.", "identity.invalid-credentials"));
}

public sealed class SignOutHandler(ISessionStore sessionStore)
{
    public Task HandleAsync(CancellationToken cancellationToken = default) => sessionStore.ClearAsync(cancellationToken);
}

public sealed class RestoreSessionHandler(
    IUserRepository users,
    IAccessTokenCodec accessTokenCodec,
    ISessionStore sessionStore)
{
    public async Task<SessionUser?> HandleAsync(CancellationToken cancellationToken = default)
    {
        var token = await sessionStore.ReadAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var claims = accessTokenCodec.Read(token);

        if (claims is null)
        {
            await sessionStore.ClearAsync(cancellationToken);
            return null;
        }

        var user = await users.FindAsync(new UserId(claims.UserId), cancellationToken);

        if (user is null || user.IsActive is false)
        {
            await sessionStore.ClearAsync(cancellationToken);
            return null;
        }

        return SessionUserMapper.FromDomain(user);
    }
}