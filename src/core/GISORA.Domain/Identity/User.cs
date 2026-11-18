using GISORA.Domain.Common;

namespace GISORA.Domain.Identity;

public sealed record UserId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static UserId New() => new(Guid.CreateVersion7());
}

public enum UserRole
{
    Administrator,
    Analyst,
    PublicViewer,
}

public sealed record UserRegistered(UserId UserId, string Email, UserRole Role) : DomainEvent;

public sealed class User : AggregateRoot<UserId>
{
    private User(
        UserId id,
        string name,
        string email,
        HashedPassword password,
        UserRole role,
        bool isActive,
        DateTimeOffset createdAt,
        DateTimeOffset? lastSignInAt)
        : base(id)
    {
        Name = ValidateName(name);
        Email = ValidateEmail(email);
        PasswordHash = ValidateSecret(password.Hash, nameof(password));
        PasswordSalt = ValidateSecret(password.Salt, nameof(password));
        Role = ValidateRole(role);
        IsActive = isActive;
        CreatedAt = createdAt;
        LastSignInAt = lastSignInAt;
    }

    public string Name { get; private set; }

    public string Email { get; private set; }

    public string PasswordHash { get; private set; }

    public string PasswordSalt { get; private set; }

    public UserRole Role { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset? LastSignInAt { get; private set; }

    public IReadOnlyCollection<AccessRight> Permissions => RoleRights.For(Role);

    public static User Create(string name, string email, HashedPassword password, UserRole role, DateTimeOffset createdAt)
    {
        var user = new User(UserId.New(), name, email, password, role, true, createdAt, null);
        user.Raise(new UserRegistered(user.Id, user.Email, user.Role));

        return user;
    }

    public void ChangeRole(UserRole role) => Role = ValidateRole(role);

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    public void ReplacePassword(HashedPassword password)
    {
        PasswordHash = ValidateSecret(password.Hash, nameof(password));
        PasswordSalt = ValidateSecret(password.Salt, nameof(password));
    }

    public void RecordSignIn(DateTimeOffset instant) => LastSignInAt = instant;

    public bool HasPermission(AccessRight permission) => Permissions.Contains(permission);

    internal static User Rehydrate(
        UserId id,
        string name,
        string email,
        string passwordHash,
        string passwordSalt,
        UserRole role,
        bool isActive,
        DateTimeOffset createdAt,
        DateTimeOffset? lastSignInAt)
        => new(id, name, email, new HashedPassword(passwordHash, passwordSalt), role, isActive, createdAt, lastSignInAt);

    private static string ValidateName(string name)
    {
        var normalized = Guard.NotNullOrWhiteSpace(name, nameof(name)).Trim();

        Guard.Condition(normalized.Length <= 120, "Nome do usuario excede 120 caracteres.", "identity.user.name-length");

        return normalized;
    }

    private static string ValidateEmail(string email)
    {
        var normalized = Guard.NotNullOrWhiteSpace(email, nameof(email)).Trim().ToLowerInvariant();

        Guard.Condition(
            normalized.Length <= 160,
            "E-mail do usuario excede 160 caracteres.",
            "identity.user.email-length");

        Guard.Condition(
            normalized.Contains('@', StringComparison.Ordinal)
            && normalized.IndexOf('@', StringComparison.Ordinal) > 0
            && normalized.IndexOf('@', StringComparison.Ordinal) < normalized.Length - 1
            && !normalized.Contains(' ', StringComparison.Ordinal),
            "E-mail do usuario invalido.",
            "identity.user.email-invalid");

        return normalized;
    }

    private static string ValidateSecret(string value, string parameterName)
        => Guard.NotNullOrWhiteSpace(value, parameterName);

    private static UserRole ValidateRole(UserRole role)
    {
        Guard.Condition(Enum.IsDefined(role), "Perfil de usuario invalido.", "identity.user.role-invalid");

        return role;
    }
}