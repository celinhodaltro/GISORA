namespace GISORA.BuildingBlocks.Domain;

/// <summary>
/// Entidade com identidade propria. Igualdade por tipo e identificador.
/// Sem construtor vazio: persistencia mapeia para o dominio de forma explicita.
/// </summary>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    protected Entity(TId id)
    {
        ArgumentNullException.ThrowIfNull(id);
        Id = id;
    }

    public TId Id { get; }

    public bool Equals(Entity<TId>? other)
        => other is not null
           && other.GetType() == GetType()
           && EqualityComparer<TId>.Default.Equals(Id, other.Id);

    public override bool Equals(object? obj) => Equals(obj as Entity<TId>);

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) => Equals(left, right);

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) => !Equals(left, right);
}