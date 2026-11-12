namespace GISORA.BuildingBlocks.Domain;

/// <summary>
/// Objeto de valor: sem identidade, imutavel e comparado pelos componentes de igualdade.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public bool Equals(ValueObject? other)
        => other is not null
           && other.GetType() == GetType()
           && GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());

    public override bool Equals(object? obj) => Equals(obj as ValueObject);

    public override int GetHashCode()
    {
        var hash = 17;
        foreach (var component in GetEqualityComponents())
        {
            hash = HashCode.Combine(hash, component);
        }

        return hash;
    }

    public static bool operator ==(ValueObject? left, ValueObject? right) => Equals(left, right);

    public static bool operator !=(ValueObject? left, ValueObject? right) => !Equals(left, right);
}