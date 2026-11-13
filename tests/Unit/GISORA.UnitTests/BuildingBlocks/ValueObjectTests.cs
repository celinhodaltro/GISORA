using GISORA.Domain.Common;

namespace GISORA.UnitTests.BuildingBlocks;

public class ValueObjectTests
{
    private sealed class Money(decimal amount, string currency) : ValueObject
    {
        public decimal Amount { get; } = amount;

        public string Currency { get; } = currency;

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }
    }

    private sealed class Weight(decimal amount, string currency) : ValueObject
    {
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return amount;
            yield return currency;
        }
    }

    [Fact]
    public void ObjectsWithSameComponentsAreEqual()
    {
        var left = new Money(10m, "BRL");
        var right = new Money(10m, "BRL");

        Assert.Equal(left, right);
        Assert.True(left == right);
        Assert.Equal(left.GetHashCode(), right.GetHashCode());
    }

    [Fact]
    public void ObjectsWithDifferentComponentsAreNotEqual()
    {
        var left = new Money(10m, "BRL");
        var right = new Money(10m, "USD");

        Assert.NotEqual(left, right);
        Assert.True(left != right);
    }

    [Fact]
    public void SameComponentsInDifferentTypesAreNotEqual()
    {
        var money = new Money(10m, "BRL");
        var weight = new Weight(10m, "BRL");

        Assert.False(money.Equals(weight));
    }
}