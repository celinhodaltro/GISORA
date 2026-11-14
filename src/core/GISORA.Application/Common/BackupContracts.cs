namespace GISORA.Application.Common;

public sealed record BackupManifest(
    int SchemaVersion,
    DateTimeOffset ExportedAt,
    IReadOnlyDictionary<string, string> Records);

public enum BackupRestoreMode
{
    Replace,
    Merge,
}

public sealed record BackupValidation(
    bool IsValid,
    IReadOnlyList<string> Errors,
    BackupManifest? Manifest,
    int CurrentSchemaVersion);

public sealed record BackupRestoreResult(
    bool Success,
    string? Error,
    int Restored,
    int Skipped);

public interface IBackupService
{
    Task<BackupManifest> ExportAsync(CancellationToken cancellationToken = default);

    Task<BackupValidation> ValidateAsync(string json, CancellationToken cancellationToken = default);

    Task<BackupRestoreResult> RestoreAsync(
        string json,
        BackupRestoreMode mode,
        CancellationToken cancellationToken = default);
}