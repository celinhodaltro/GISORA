using GISORA.Infrastructure.LocalStorage.UnitTests.Fakes;

namespace GISORA.Infrastructure.LocalStorage.UnitTests;

public class StorageSchemaMigratorTests
{
    private sealed class CountingMigration(int toVersion) : IStorageMigration
    {
        public int ApplyCount { get; private set; }

        public int ToVersion => toVersion;

        public Task ApplyAsync(ILocalStore store, CancellationToken cancellationToken = default)
        {
            ApplyCount++;

            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task AppliesPendingMigrationsInOrder()
    {
        var store = new InMemoryLocalStore();
        var first = new CountingMigration(2);
        var second = new CountingMigration(3);
        var migrator = new StorageSchemaMigrator(store, [second, first]);

        var version = await migrator.MigrateAsync();

        Assert.Equal(3, version);
        Assert.Equal(1, first.ApplyCount);
        Assert.Equal(1, second.ApplyCount);
    }

    [Fact]
    public async Task SecondRunDoesNotReapply()
    {
        var store = new InMemoryLocalStore();
        var migration = new CountingMigration(2);
        var migrator = new StorageSchemaMigrator(store, [migration]);

        await migrator.MigrateAsync();
        var version = await migrator.MigrateAsync();

        Assert.Equal(2, version);
        Assert.Equal(1, migration.ApplyCount);
    }

    [Fact]
    public async Task PersistsBaselineWhenNoMigrationExists()
    {
        var store = new InMemoryLocalStore();
        var migrator = new StorageSchemaMigrator(store, []);

        var version = await migrator.MigrateAsync();

        Assert.Equal(StorageSchemaMigrator.BaselineVersion, version);
        Assert.Equal(version, await store.GetAsync<int?>("meta", "schema"));
    }

    [Fact]
    public async Task SkipsMigrationsBelowCurrentVersion()
    {
        var store = new InMemoryLocalStore();
        var old = new CountingMigration(1);
        var current = new CountingMigration(5);
        var migrator = new StorageSchemaMigrator(store, [old, current]);

        await migrator.MigrateAsync();
        await migrator.MigrateAsync();

        Assert.Equal(0, old.ApplyCount);
        Assert.Equal(1, current.ApplyCount);
    }

    [Fact]
    public async Task DuplicateVersionsAreRejected()
    {
        var store = new InMemoryLocalStore();
        var migrator = new StorageSchemaMigrator(store, [new CountingMigration(2), new CountingMigration(2)]);

        await Assert.ThrowsAsync<InvalidOperationException>(() => migrator.MigrateAsync());
    }
}