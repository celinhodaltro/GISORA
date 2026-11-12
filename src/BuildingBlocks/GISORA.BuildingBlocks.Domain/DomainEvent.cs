namespace GISORA.BuildingBlocks.Domain;

/// <summary>
/// Base para eventos de dominio, com instante de ocorrencia em UTC.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    protected DomainEvent()
        : this(DateTimeOffset.UtcNow)
    {
    }

    protected DomainEvent(DateTimeOffset occurredOn) => OccurredOn = occurredOn.ToUniversalTime();

    public DateTimeOffset OccurredOn { get; init; }
}