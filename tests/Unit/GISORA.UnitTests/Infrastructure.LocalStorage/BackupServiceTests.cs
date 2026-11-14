using GISORA.Application.Common;
using GISORA.Infrastructure.Persistence;
using GISORA.UnitTests.Infrastructure.LocalStorage.Fakes;

using Microsoft.JSInterop;

namespace GISORA.UnitTests.Infrastructure.LocalStorage;

public class BackupServiceTests
{
    private static async Task<FakeJsStorage> SeedAsync()
    {
        var storage = new FakeJsStorage();
        await storage.SetItemAsync("gisora:meta:schema", "1", CancellationToken.None);
        await storage.SetItemAsync("gisora:properties:property-1", "{\"id\":1}", CancellationToken.None);

        return storage;
    }

    private static string Backup(int schemaVersion, Dictionary<string, string> records)
        => System.Text.Json.JsonSerializer.Serialize(new
        {
            schemaVersion,
            exportedAt = "2026-11-14T12:00:00Z",
            records,
        });

    [Fact]
    public async Task ExportIncludesSchemaAndRecords()
    {
        var storage = await SeedAsync();
        var service = new LocalStorageBackupService(storage);

        var manifest = await service.ExportAsync();

        Assert.Equal(1, manifest.SchemaVersion);
        Assert.Equal(2, manifest.Records.Count);
        Assert.Contains("gisora:properties:property-1", manifest.Records.Keys);
    }

    [Fact]
    public async Task ValidateRejectsInvalidJson()
    {
        var service = new LocalStorageBackupService(await SeedAsync());

        var validation = await service.ValidateAsync("{not-json");

        Assert.False(validation.IsValid);
        Assert.Contains("JSON invalido.", validation.Errors);
    }

    [Fact]
    public async Task ValidateRejectsIncompatibleSchema()
    {
        var service = new LocalStorageBackupService(await SeedAsync());
        var json = Backup(99, new Dictionary<string, string> { ["gisora:meta:schema"] = "99" });

        var validation = await service.ValidateAsync(json);

        Assert.False(validation.IsValid);
        Assert.Contains(validation.Errors, error => error.Contains("incompativel", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ValidateRejectsKeysOutsideGisoraNamespace()
    {
        var service = new LocalStorageBackupService(await SeedAsync());
        var json = Backup(1, new Dictionary<string, string> { ["other:key"] = "value" });

        var validation = await service.ValidateAsync(json);

        Assert.False(validation.IsValid);
        Assert.Contains(validation.Errors, error => error.Contains("fora do namespace", StringComparison.Ordinal));
    }

    [Fact]
    public async Task RestoreReplaceRemovesCurrentData()
    {
        var storage = await SeedAsync();
        var service = new LocalStorageBackupService(storage);
        var json = Backup(1, new Dictionary<string, string>
        {
            ["gisora:meta:schema"] = "1",
            ["gisora:properties:property-2"] = "{\"id\":2}",
        });

        var result = await service.RestoreAsync(json, BackupRestoreMode.Replace);

        Assert.True(result.Success);
        Assert.Equal(2, result.Restored);
        Assert.Null(await storage.GetItemAsync("gisora:properties:property-1", CancellationToken.None));
        Assert.Equal("{\"id\":2}", await storage.GetItemAsync("gisora:properties:property-2", CancellationToken.None));
    }

    [Fact]
    public async Task RestoreMergePreservesExistingRecords()
    {
        var storage = await SeedAsync();
        var service = new LocalStorageBackupService(storage);
        var json = Backup(1, new Dictionary<string, string>
        {
            ["gisora:properties:property-1"] = "{\"id\":99}",
            ["gisora:properties:property-2"] = "{\"id\":2}",
        });

        var result = await service.RestoreAsync(json, BackupRestoreMode.Merge);

        Assert.True(result.Success);
        Assert.Equal(1, result.Restored);
        Assert.Equal(1, result.Skipped);
        Assert.Equal("{\"id\":1}", await storage.GetItemAsync("gisora:properties:property-1", CancellationToken.None));
        Assert.Equal("{\"id\":2}", await storage.GetItemAsync("gisora:properties:property-2", CancellationToken.None));
    }

    [Fact]
    public async Task RestoreRollsBackWhenStorageFails()
    {
        var storage = await OnceFailingStorage.CreateAsync();
        var service = new LocalStorageBackupService(storage);
        var json = Backup(1, new Dictionary<string, string>
        {
            ["gisora:properties:property-2"] = "{\"id\":2}",
        });

        var result = await service.RestoreAsync(json, BackupRestoreMode.Replace);

        Assert.False(result.Success);
        Assert.Contains("cheio", result.Error!, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("{\"id\":1}", await storage.GetItemAsync("gisora:properties:property-1", CancellationToken.None));
        Assert.Null(await storage.GetItemAsync("gisora:properties:property-2", CancellationToken.None));
    }

    private sealed class OnceFailingStorage : IJsStorage
    {
        private readonly Dictionary<string, string> _items = [];
        private bool _failNextSet;

        public static async Task<OnceFailingStorage> CreateAsync()
        {
            var storage = new OnceFailingStorage();
            await storage.SetItemAsync("gisora:meta:schema", "1", CancellationToken.None);
            await storage.SetItemAsync("gisora:properties:property-1", "{\"id\":1}", CancellationToken.None);
            storage._failNextSet = true;

            return storage;
        }

        public ValueTask<bool> IsAvailableAsync(CancellationToken cancellationToken) => ValueTask.FromResult(true);

        public ValueTask<string?> GetItemAsync(string key, CancellationToken cancellationToken)
            => ValueTask.FromResult(_items.TryGetValue(key, out var value) ? value : null);

        public ValueTask SetItemAsync(string key, string value, CancellationToken cancellationToken)
        {
            if (_failNextSet)
            {
                _failNextSet = false;
                throw new JSException("Quota exceeded");
            }

            _items[key] = value;

            return ValueTask.CompletedTask;
        }

        public ValueTask RemoveItemAsync(string key, CancellationToken cancellationToken)
        {
            _items.Remove(key);

            return ValueTask.CompletedTask;
        }

        public ValueTask<string[]> GetKeysAsync(string prefix, CancellationToken cancellationToken)
            => ValueTask.FromResult(_items.Keys.Where(key => key.StartsWith(prefix, StringComparison.Ordinal)).ToArray());

        public ValueTask RemoveKeysAsync(string prefix, CancellationToken cancellationToken)
        {
            foreach (var key in _items.Keys.Where(key => key.StartsWith(prefix, StringComparison.Ordinal)).ToArray())
            {
                _items.Remove(key);
            }

            return ValueTask.CompletedTask;
        }
    }
}