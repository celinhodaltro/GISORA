namespace GISORA.BuildingBlocks.Domain;

public abstract record DomainEvent : IDomainEvent
{
    protected DomainEvent()
        : this(DateTimeOffset.UtcNow)
    {
    }

    protected DomainEvent(DateTimeOffset occurredOn) => OccurredOn = occurredOn.ToUniversalTime();

    public DateTimeOffset OccurredOn { get; init; }
}