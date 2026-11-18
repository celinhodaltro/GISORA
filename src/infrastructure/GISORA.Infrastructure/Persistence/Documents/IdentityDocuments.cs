using GISORA.Domain.Identity;

namespace GISORA.Infrastructure.Persistence.Documents;

internal sealed class UserDocument
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string PasswordHash { get; init; } = string.Empty;

    public string PasswordSalt { get; init; } = string.Empty;

    public UserRole Role { get; init; }

    public bool IsActive { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? LastSignInAt { get; init; }

    public User ToDomain() => User.Rehydrate(
        new UserId(Id),
        Name,
        Email,
        PasswordHash,
        PasswordSalt,
        Role,
        IsActive,
        CreatedAt,
        LastSignInAt);

    public static UserDocument FromDomain(User user) => new()
    {
        Id = user.Id.Value,
        Name = user.Name,
        Email = user.Email,
        PasswordHash = user.PasswordHash,
        PasswordSalt = user.PasswordSalt,
        Role = user.Role,
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt,
        LastSignInAt = user.LastSignInAt,
    };
}