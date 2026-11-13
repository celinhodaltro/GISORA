using GISORA.Domain.Common;

namespace GISORA.UnitTests.BuildingBlocks;

public class AggregateRootTests
{
    private sealed record PropertyRegistered(Guid PropertyId, DateTimeOffset OccurredOn) : DomainEvent(OccurredOn);

    private sealed class Property(Guid id) : AggregateRoot<Guid>(id)
    {
        public void Register() => Raise(new PropertyRegistered(Id, new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero)));
    }

    [Fact]
    public void RaisesAndDequeuesDomainEvents()
    {
        var property = new Property(Guid.CreateVersion7());

        property.Register();

        var events = property.DequeueDomainEvents();

        var domainEvent = Assert.Single(events);
        Assert.IsType<PropertyRegistered>(domainEvent);
        Assert.Empty(property.DomainEvents);
    }

    [Fact]
    public void SecondDequeueReturnsNothing()
    {
        var property = new Property(Guid.CreateVersion7());
        property.Register();

        property.DequeueDomainEvents();

        Assert.Empty(property.DequeueDomainEvents());
    }

    [Fact]
    public void EventTimestampIsNormalizedToUtc()
    {
        var local = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.FromHours(-3));

        var domainEvent = new PropertyRegistered(Guid.CreateVersion7(), local);

        Assert.Equal(TimeSpan.Zero, domainEvent.OccurredOn.Offset);
        Assert.Equal(local.UtcDateTime, domainEvent.OccurredOn.UtcDateTime);
    }
}