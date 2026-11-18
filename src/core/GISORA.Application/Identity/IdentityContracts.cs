using GISORA.Domain.Identity;

namespace GISORA.Application.Identity;

public static class IdentityRoles
{
    public const string Administrator = nameof(UserRole.Administrator);

    public const string Analyst = nameof(UserRole.Analyst);

    public const string PublicViewer = nameof(UserRole.PublicViewer);

    public const string Management = Administrator + "," + Analyst;
}

public sealed record SessionUser(
    Guid Id,
    string Name,
    string Email,
    UserRole Role,
    IReadOnlyCollection<AccessRight> Permissions)
{
    public bool HasPermission(AccessRight permission) => Permissions.Contains(permission);
}

public sealed record AccessToken(string Value, DateTimeOffset ExpiresAt);

public sealed record AccessTokenClaims(Guid UserId, DateTimeOffset ExpiresAt);

public interface IAccessTokenCodec
{
    AccessToken Issue(SessionUser user, DateTimeOffset issuedAt);

    AccessTokenClaims? Read(string token);
}

public interface ISessionStore
{
    Task<string?> ReadAsync(CancellationToken cancellationToken = default);

    Task WriteAsync(AccessToken token, CancellationToken cancellationToken = default);

    Task ClearAsync(CancellationToken cancellationToken = default);
}

public sealed record UserListItem(
    UserId Id,
    string Name,
    string Email,
    UserRole Role,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastSignInAt);

public interface IUserQueries
{
    Task<IReadOnlyList<UserListItem>> ListAsync(CancellationToken cancellationToken = default);
}

public static class IdentitySeedAccounts
{
    public const string Password = "Gisora@2026";

    public const string AdministratorEmail = "admin@gisora.gov.br";

    public const string AnalystEmail = "analista@gisora.gov.br";

    public const string PublicViewerEmail = "consulta@gisora.gov.br";
}

internal static class SessionUserMapper
{
    public static SessionUser FromDomain(User user)
        => new(user.Id.Value, user.Name, user.Email, user.Role, user.Permissions);
}