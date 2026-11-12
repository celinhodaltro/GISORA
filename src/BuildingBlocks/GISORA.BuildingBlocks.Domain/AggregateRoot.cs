namespace GISORA.BuildingBlocks.Domain;

/// <summary>
/// Raiz de agregado: ponto unico de acesso as entidades internas e origem dos eventos de dominio.
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected AggregateRoot(TId id)
        : base(id)
    {
    }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void Raise(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Remove e devolve os eventos pendentes. A publicacao e responsabilidade da Application.
    /// </summary>
    public IReadOnlyList<IDomainEvent> DequeueDomainEvents()
    {
        if (_domainEvents.Count == 0)
        {
            return [];
        }

        var events = _domainEvents.ToArray();
        _domainEvents.Clear();
        return events;
    }
}