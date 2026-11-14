using GISORA.Domain.Properties;

namespace GISORA.Application.Properties;

public sealed record GeometryImportRecord(
    Guid Id,
    PropertyId PropertyId,
    string Registration,
    string Origin,
    string SourceCrs,
    string TargetCrs,
    int FeatureCount,
    DateTimeOffset ImportedAt);

public interface IGeometryImportLog
{
    Task RecordAsync(GeometryImportRecord record, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GeometryImportRecord>> ListAsync(CancellationToken cancellationToken = default);
}