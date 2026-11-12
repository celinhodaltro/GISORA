using GISORA.BuildingBlocks.Domain;
using GISORA.Territory.Domain;

namespace GISORA.Territory.UnitTests;

public class IbgeCodeTests
{
    [Fact]
    public void AcceptsSevenDigitsWithValidState()
    {
        var code = IbgeCode.Parse("2611903");

        Assert.Equal("2611903", code.Value);
        Assert.Equal(26, code.StateCode);
    }

    [Theory]
    [InlineData("261190")]
    [InlineData("26119031")]
    [InlineData("26119O3")]
    public void RejectsInvalidFormat(string value)
    {
        var exception = Assert.Throws<DomainException>(() => IbgeCode.Parse(value));

        Assert.Equal("territory.ibge.format", exception.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void RejectsMissingValue(string? value)
    {
        var exception = Assert.Throws<DomainException>(() => IbgeCode.Parse(value));

        Assert.Equal("guard.empty", exception.Code);
    }

    [Fact]
    public void RejectsInvalidStateCode()
    {
        var exception = Assert.Throws<DomainException>(() => IbgeCode.Parse("9911903"));

        Assert.Equal("territory.ibge.state", exception.Code);
    }

    [Fact]
    public void CodesWithSameValueAreEqual()
    {
        Assert.Equal(IbgeCode.Parse("2611903"), IbgeCode.Parse("2611903"));
    }
}