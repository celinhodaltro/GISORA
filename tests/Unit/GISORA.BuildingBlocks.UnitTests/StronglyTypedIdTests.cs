using GISORA.BuildingBlocks.Domain;

namespace GISORA.BuildingBlocks.UnitTests;

public class StronglyTypedIdTests
{
    private sealed record PropertyId(Guid Value) : StronglyTypedId<Guid>(Value);

    private sealed record TaxpayerId(Guid Value) : StronglyTypedId<Guid>(Value);

    [Fact]
    public void IdsOfSameTypeWithSameValueAreEqual()
    {
        var value = Guid.CreateVersion7();

        Assert.Equal(new PropertyId(value), new PropertyId(value));
        Assert.Equal(new PropertyId(value).GetHashCode(), new PropertyId(value).GetHashCode());
    }

    [Fact]
    public void IdsOfSameTypeWithDifferentValuesAreNotEqual()
    {
        Assert.NotEqual(new PropertyId(Guid.CreateVersion7()), new PropertyId(Guid.CreateVersion7()));
    }

    [Fact]
    public void DifferentIdTypesAreDifferentTypesEvenWithSameValue()
    {
        var value = Guid.CreateVersion7();

        Assert.NotEqual<object>(new PropertyId(value), new TaxpayerId(value));
    }

    [Fact]
    public void IdExposesUnderlyingValue()
    {
        var value = Guid.CreateVersion7();

        Assert.Equal(value, new PropertyId(value).Value);
    }
}