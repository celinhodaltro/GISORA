using GISORA.BuildingBlocks.Domain;

namespace GISORA.BuildingBlocks.UnitTests;

public class GuardTests
{
    [Fact]
    public void NotNullRejectsNull()
    {
        var exception = Assert.Throws<DomainException>(() => Guard.NotNull<string>(null, "name"));

        Assert.Equal("guard.null", exception.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void NotNullOrWhiteSpaceRejectsBlank(string value)
    {
        var exception = Assert.Throws<DomainException>(() => Guard.NotNullOrWhiteSpace(value, "name"));

        Assert.Equal("guard.empty", exception.Code);
    }

    [Fact]
    public void NotNullOrWhiteSpaceReturnsValue()
    {
        Assert.Equal("Passira", Guard.NotNullOrWhiteSpace("Passira", "name"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void PositiveRejectsNonPositive(int value)
    {
        Assert.Throws<DomainException>(() => Guard.Positive(value, "area"));
    }

    [Fact]
    public void PositiveAcceptsPositiveDecimal()
    {
        Assert.Equal(12.5m, Guard.Positive(12.5m, "area"));
    }

    [Fact]
    public void ConditionThrowsWithGivenCode()
    {
        var exception = Assert.Throws<DomainException>(() => Guard.Condition(false, "falhou", "custom.code"));

        Assert.Equal("custom.code", exception.Code);
    }
}