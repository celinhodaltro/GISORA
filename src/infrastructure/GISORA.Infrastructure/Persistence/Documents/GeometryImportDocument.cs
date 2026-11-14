using GISORA.Application.Properties;

namespace GISORA.Infrastructure.Persistence.Documents;

internal sealed class GeometryImportDocument
{
    public Guid Id { get; init; }

    public Guid PropertyId { get; init; }

    public string Registration { get; init; } = string.Empty;

    public string Origin { get; init; } = string.Empty;

    public string SourceCrs { get; init; } = string.Empty;

    public string TargetCrs { get; init; } = string.Empty;

    public int FeatureCount { get; init; }

    public DateTimeOffset ImportedAt { get; init; }

    public GeometryImportRecord ToDomain() => new(
        Id,
        new GISORA.Domain.Properties.PropertyId(PropertyId),
        Registration,
        Origin,
        SourceCrs,
        TargetCrs,
        FeatureCount,
        ImportedAt);

    public static GeometryImportDocument FromDomain(GeometryImportRecord record) => new()
    {
        Id = record.Id,
        PropertyId = record.PropertyId.Value,
        Registration = record.Registration,
        Origin = record.Origin,
        SourceCrs = record.SourceCrs,
        TargetCrs = record.TargetCrs,
        FeatureCount = record.FeatureCount,
        ImportedAt = record.ImportedAt,
    };
}