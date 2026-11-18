using GISORA.Application.Identity;
using GISORA.Domain.Identity;

namespace GISORA.Infrastructure.Persistence.Repositories;

public sealed class LocalStorageUserQueries(IUserRepository users) : IUserQueries
{
    public async Task<IReadOnlyList<UserListItem>> ListAsync(CancellationToken cancellationToken = default)
    {
        var stored = await users.ListAsync(cancellationToken);

        return stored
            .OrderBy(user => user.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(user => new UserListItem(
                user.Id,
                user.Name,
                user.Email,
                user.Role,
                user.IsActive,
                user.CreatedAt,
                user.LastSignInAt))
            .ToArray();
    }
}