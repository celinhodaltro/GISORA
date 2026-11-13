namespace GISORA.Domain.Common;

public interface IDomainEvent
{
    DateTimeOffset OccurredOn { get; }
}