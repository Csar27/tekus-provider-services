namespace Tekus.Domain.Common;

/// <summary>
/// Base class for value objects: immutable, compared by the value of their components and
/// with no identity of their own.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>Returns every component that takes part in the equality comparison.</summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public bool Equals(ValueObject? other)
        => other is not null
           && other.GetType() == GetType()
           && GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());

    public override bool Equals(object? obj) => Equals(obj as ValueObject);

    public override int GetHashCode()
        => GetEqualityComponents().Aggregate(
            seed: 17,
            (accumulator, component) => (accumulator * 31) + (component?.GetHashCode() ?? 0));

    public static bool operator ==(ValueObject? left, ValueObject? right) => Equals(left, right);

    public static bool operator !=(ValueObject? left, ValueObject? right) => !Equals(left, right);
}
