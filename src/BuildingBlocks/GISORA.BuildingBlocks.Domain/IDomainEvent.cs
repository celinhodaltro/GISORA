namespace GISORA.BuildingBlocks.Domain;

public interface IDomainEvent
{
    DateTimeOffset OccurredOn { get; }
}