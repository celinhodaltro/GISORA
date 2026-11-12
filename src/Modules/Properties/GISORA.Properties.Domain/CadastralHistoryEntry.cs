using GISORA.BuildingBlocks.Domain;

namespace GISORA.Properties.Domain;

public sealed class CadastralHistoryEntry : Entity<Guid>
{
    internal CadastralHistoryEntry(Guid id, DateTimeOffset changedAt, string field, string? previousValue, string? currentValue)
        : base(id)
    {
        ChangedAt = changedAt.ToUniversalTime();
        Field = Guard.NotNullOrWhiteSpace(field, nameof(field));
        PreviousValue = previousValue;
        CurrentValue = currentValue;
    }

    public DateTimeOffset ChangedAt { get; }

    public string Field { get; }

    public string? PreviousValue { get; }

    public string? CurrentValue { get; }
}