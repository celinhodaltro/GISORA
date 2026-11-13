namespace GISORA.Infrastructure.Persistence;

public interface IStorageMigration
{
    int ToVersion { get; }

    Task ApplyAsync(ILocalStore store, CancellationToken cancellationToken = default);
}

public sealed class StorageSchemaMigrator(ILocalStore store, IEnumerable<IStorageMigration> migrations)
{
    public const int BaselineVersion = 1;

    private const string MetaNamespace = "meta";
    private const string SchemaKey = "schema";

    public async Task<int> MigrateAsync(CancellationToken cancellationToken = default)
    {
        var current = await store.GetAsync<int?>(MetaNamespace, SchemaKey, cancellationToken) ?? BaselineVersion;
        var pending = migrations
            .Where(migration => migration.ToVersion > current)
            .OrderBy(migration => migration.ToVersion)
            .ToArray();

        EnsureUniqueVersions(pending);

        foreach (var migration in pending)
        {
            await migration.ApplyAsync(store, cancellationToken);
            await store.SetAsync(MetaNamespace, SchemaKey, migration.ToVersion, cancellationToken);
            current = migration.ToVersion;
        }

        if (pending.Length == 0)
        {
            await store.SetAsync(MetaNamespace, SchemaKey, current, cancellationToken);
        }

        return current;
    }

    private static void EnsureUniqueVersions(IStorageMigration[] pending)
    {
        var versions = pending.Select(migration => migration.ToVersion).ToArray();

        if (versions.Distinct().Count() != versions.Length)
        {
            throw new InvalidOperationException("Existem migracoes duplicadas para a mesma versao de schema.");
        }
    }
}