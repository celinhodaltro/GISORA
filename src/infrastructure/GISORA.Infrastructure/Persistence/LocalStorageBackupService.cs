using System.Text.Json;

using GISORA.Application.Common;

using Microsoft.JSInterop;

namespace GISORA.Infrastructure.Persistence;

public sealed class LocalStorageBackupService : IBackupService
{
    private const string RootPrefix = "gisora:";
    private const string SchemaKey = "gisora:meta:schema";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
    };

    private readonly IJsStorage _storage;

    public LocalStorageBackupService(IJSRuntime jsRuntime)
        : this(new JsStorage(jsRuntime))
    {
    }

    internal LocalStorageBackupService(IJsStorage storage) => _storage = storage;

    public async Task<BackupManifest> ExportAsync(CancellationToken cancellationToken = default)
    {
        var schema = await ReadSchemaAsync(cancellationToken);
        var keys = await _storage.GetKeysAsync(RootPrefix, cancellationToken);
        var records = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var key in keys.Order(StringComparer.Ordinal))
        {
            var value = await _storage.GetItemAsync(key, cancellationToken);

            if (value is not null)
            {
                records[key] = value;
            }
        }

        return new BackupManifest(schema, DateTimeOffset.UtcNow, records);
    }

    public async Task<BackupValidation> ValidateAsync(string json, CancellationToken cancellationToken = default)
    {
        var current = await ReadSchemaAsync(cancellationToken);
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(json))
        {
            return new BackupValidation(false, ["Arquivo de backup vazio."], null, current);
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Object)
            {
                return new BackupValidation(false, ["Backup deve ser um objeto JSON."], null, current);
            }

            if (!root.TryGetProperty("schemaVersion", out var schemaElement)
                || schemaElement.ValueKind != JsonValueKind.Number
                || !schemaElement.TryGetInt32(out var schemaVersion))
            {
                errors.Add("Campo schemaVersion ausente ou invalido.");
                schemaVersion = 0;
            }

            if (!root.TryGetProperty("exportedAt", out var exportedElement)
                || exportedElement.ValueKind != JsonValueKind.String
                || !exportedElement.TryGetDateTimeOffset(out var exportedAt))
            {
                errors.Add("Campo exportedAt ausente ou invalido.");
                exportedAt = default;
            }

            var records = new Dictionary<string, string>(StringComparer.Ordinal);

            if (!root.TryGetProperty("records", out var recordsElement)
                || recordsElement.ValueKind != JsonValueKind.Object)
            {
                errors.Add("Campo records deve ser um objeto de strings.");
            }
            else
            {
                foreach (var property in recordsElement.EnumerateObject())
                {
                    if (!property.Name.StartsWith(RootPrefix, StringComparison.Ordinal))
                    {
                        errors.Add($"Registro fora do namespace do GISORA: {property.Name}.");
                        continue;
                    }

                    if (property.Value.ValueKind != JsonValueKind.String)
                    {
                        errors.Add($"Registro '{property.Name}' deve ser texto.");
                        continue;
                    }

                    records[property.Name] = property.Value.GetString() ?? string.Empty;
                }
            }

            if (schemaVersion > 0 && schemaVersion != current)
            {
                errors.Add($"Versao do backup ({schemaVersion}) incompativel com a versao atual ({current}).");
            }

            if (errors.Count > 0)
            {
                return new BackupValidation(false, errors, null, current);
            }

            return new BackupValidation(
                true,
                [],
                new BackupManifest(schemaVersion, exportedAt, records),
                current);
        }
        catch (JsonException)
        {
            return new BackupValidation(false, ["JSON invalido."], null, current);
        }
    }

    public async Task<BackupRestoreResult> RestoreAsync(
        string json,
        BackupRestoreMode mode,
        CancellationToken cancellationToken = default)
    {
        var validation = await ValidateAsync(json, cancellationToken);

        if (validation.IsValid is false || validation.Manifest is null)
        {
            return new BackupRestoreResult(false, string.Join(" ", validation.Errors), 0, 0);
        }

        var manifest = validation.Manifest;
        var snapshot = await ReadAllAsync(cancellationToken);
        var skipped = 0;

        try
        {
            if (mode == BackupRestoreMode.Replace)
            {
                await _storage.RemoveKeysAsync(RootPrefix, cancellationToken);
            }

            foreach (var record in manifest.Records)
            {
                if (mode == BackupRestoreMode.Merge && snapshot.ContainsKey(record.Key))
                {
                    skipped++;
                    continue;
                }

                await _storage.SetItemAsync(record.Key, record.Value, cancellationToken);
            }

            return new BackupRestoreResult(true, null, manifest.Records.Count - skipped, skipped);
        }
        catch (JSException)
        {
            await RollbackAsync(snapshot, cancellationToken);

            return new BackupRestoreResult(
                false,
                "Nao foi possivel restaurar o backup. O armazenamento local pode estar cheio.",
                0,
                0);
        }
    }

    private async Task<Dictionary<string, string>> ReadAllAsync(CancellationToken cancellationToken)
    {
        var keys = await _storage.GetKeysAsync(RootPrefix, cancellationToken);
        var values = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var key in keys)
        {
            var value = await _storage.GetItemAsync(key, cancellationToken);

            if (value is not null)
            {
                values[key] = value;
            }
        }

        return values;
    }

    private async Task RollbackAsync(Dictionary<string, string> snapshot, CancellationToken cancellationToken)
    {
        try
        {
            await _storage.RemoveKeysAsync(RootPrefix, cancellationToken);

            foreach (var record in snapshot)
            {
                await _storage.SetItemAsync(record.Key, record.Value, cancellationToken);
            }
        }
        catch (JSException)
        {
        }
    }

    private async Task<int> ReadSchemaAsync(CancellationToken cancellationToken)
    {
        var raw = await _storage.GetItemAsync(SchemaKey, cancellationToken);

        return int.TryParse(raw, out var schema) ? schema : StorageSchemaMigrator.BaselineVersion;
    }
}