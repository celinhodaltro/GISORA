using GISORA.Application.Common;
using GISORA.Domain.Common;
using GISORA.Domain.Identity;

namespace GISORA.Application.Identity.Commands;

public sealed record RegisterUserCommand(string Name, string Email, string Password, UserRole Role);

public sealed class RegisterUserHandler(IUserRepository users, IPasswordHasher passwordHasher)
{
    public const int MinimumPasswordLength = 8;

    public async Task<Result<UserId>> HandleAsync(RegisterUserCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Password) || command.Password.Length < MinimumPasswordLength)
        {
            return Result.Fail<UserId>(
                Error.Validation($"A senha deve ter ao menos {MinimumPasswordLength} caracteres.", "identity.password-length"));
        }

        try
        {
            if (await users.EmailExistsAsync(command.Email, cancellationToken))
            {
                return Result.Fail<UserId>(Error.Conflict("Ja existe usuario com este e-mail.", "identity.email-duplicated"));
            }

            var user = User.Create(
                command.Name,
                command.Email,
                passwordHasher.Hash(command.Password),
                command.Role,
                DateTimeOffset.UtcNow);

            await users.AddAsync(user, cancellationToken);

            return Result.Ok(user.Id);
        }
        catch (DomainException exception)
        {
            return Result.Fail<UserId>(Error.Validation(exception.Message, exception.Code));
        }
    }
}

public sealed record UpdateUserCommand(Guid UserId, UserRole Role, bool IsActive);

public sealed class UpdateUserHandler(IUserRepository users)
{
    public async Task<Result> HandleAsync(UpdateUserCommand command, CancellationToken cancellationToken = default)
    {
        var user = await users.FindAsync(new UserId(command.UserId), cancellationToken);

        if (user is null)
        {
            return Result.Fail(Error.NotFound("Usuario nao encontrado.", "identity.user.not-found"));
        }

        var losesAdministrator = user.Role == UserRole.Administrator
            && (command.Role != UserRole.Administrator || command.IsActive is false);

        if (losesAdministrator && await users.CountActiveAdministratorsAsync(cancellationToken) <= 1)
        {
            return Result.Fail(
                Error.Conflict("E obrigatorio manter ao menos um administrador ativo.", "identity.last-administrator"));
        }

        try
        {
            user.ChangeRole(command.Role);

            if (command.IsActive)
            {
                user.Activate();
            }
            else
            {
                user.Deactivate();
            }

            await users.UpdateAsync(user, cancellationToken);

            return Result.Ok();
        }
        catch (DomainException exception)
        {
            return Result.Fail(Error.Validation(exception.Message, exception.Code));
        }
    }
}

public sealed record ResetUserPasswordCommand(Guid UserId, string Password);

public sealed class ResetUserPasswordHandler(IUserRepository users, IPasswordHasher passwordHasher)
{
    public async Task<Result> HandleAsync(ResetUserPasswordCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Password) || command.Password.Length < RegisterUserHandler.MinimumPasswordLength)
        {
            return Result.Fail(
                Error.Validation(
                    $"A senha deve ter ao menos {RegisterUserHandler.MinimumPasswordLength} caracteres.",
                    "identity.password-length"));
        }

        var user = await users.FindAsync(new UserId(command.UserId), cancellationToken);

        if (user is null)
        {
            return Result.Fail(Error.NotFound("Usuario nao encontrado.", "identity.user.not-found"));
        }

        user.ReplacePassword(passwordHasher.Hash(command.Password));
        await users.UpdateAsync(user, cancellationToken);

        return Result.Ok();
    }
}

public sealed class EnsureIdentityUsersHandler(IUserRepository users, IPasswordHasher passwordHasher)
{
    public async Task<int> HandleAsync(CancellationToken cancellationToken = default)
    {
        if ((await users.ListAsync(cancellationToken)).Count > 0)
        {
            return 0;
        }

        var accounts = new[]
        {
            ("Administrador", IdentitySeedAccounts.AdministratorEmail, UserRole.Administrator),
            ("Analista", IdentitySeedAccounts.AnalystEmail, UserRole.Analyst),
            ("Consulta publica", IdentitySeedAccounts.PublicViewerEmail, UserRole.PublicViewer),
        };

        var now = DateTimeOffset.UtcNow;

        foreach (var (name, email, role) in accounts)
        {
            var user = User.Create(name, email, passwordHasher.Hash(IdentitySeedAccounts.Password), role, now);
            await users.AddAsync(user, cancellationToken);
        }

        return accounts.Length;
    }
}