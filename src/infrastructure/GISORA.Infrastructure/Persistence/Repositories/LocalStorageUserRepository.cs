using GISORA.Domain.Identity;
using GISORA.Infrastructure.Persistence.Documents;

namespace GISORA.Infrastructure.Persistence.Repositories;

public sealed class LocalStorageUserRepository(ILocalStore store) : IUserRepository
{
    private const string IdsKey = "user-ids";

    public async Task<User?> FindAsync(UserId id, CancellationToken cancellationToken = default)
    {
        var document = await store.GetAsync<UserDocument>(StorageNamespaces.Identity, Key(id), cancellationToken);

        return document?.ToDomain();
    }

    public async Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        var users = await ListAsync(cancellationToken);

        return users.FirstOrDefault(user => user.Email == normalized);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        var users = await ListAsync(cancellationToken);

        return users.Any(user => user.Email == normalized);
    }

    public async Task<int> CountActiveAdministratorsAsync(CancellationToken cancellationToken = default)
    {
        var users = await ListAsync(cancellationToken);

        return users.Count(user => user.IsActive && user.Role == UserRole.Administrator);
    }

    public async Task<IReadOnlyList<User>> ListAsync(CancellationToken cancellationToken = default)
    {
        var ids = await ReadIdsAsync(cancellationToken);
        var users = new List<User>(ids.Count);

        foreach (var id in ids)
        {
            var document = await store.GetAsync<UserDocument>(
                StorageNamespaces.Identity,
                Key(new UserId(id)),
                cancellationToken);

            if (document is not null)
            {
                users.Add(document.ToDomain());
            }
        }

        return users;
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (await EmailExistsAsync(user.Email, cancellationToken))
        {
            throw new StorageException("Ja existe usuario com este e-mail no armazenamento local.");
        }

        await store.SetAsync(
            StorageNamespaces.Identity,
            Key(user.Id),
            UserDocument.FromDomain(user),
            cancellationToken);

        var ids = await ReadIdsAsync(cancellationToken);

        if (ids.Contains(user.Id.Value) is false)
        {
            ids.Add(user.Id.Value);
            await store.SetAsync(StorageNamespaces.Identity, IdsKey, ids, cancellationToken);
        }
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (await store.ExistsAsync(StorageNamespaces.Identity, Key(user.Id), cancellationToken) is false)
        {
            throw new StorageException($"Usuario '{user.Id}' nao encontrado no armazenamento local.");
        }

        await store.SetAsync(
            StorageNamespaces.Identity,
            Key(user.Id),
            UserDocument.FromDomain(user),
            cancellationToken);
    }

    private static string Key(UserId id) => $"user-{id.Value}";

    private async Task<List<Guid>> ReadIdsAsync(CancellationToken cancellationToken)
        => await store.GetAsync<List<Guid>>(StorageNamespaces.Identity, IdsKey, cancellationToken) ?? [];
}