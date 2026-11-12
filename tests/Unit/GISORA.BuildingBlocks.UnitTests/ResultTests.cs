using GISORA.BuildingBlocks.Application;

namespace GISORA.BuildingBlocks.UnitTests;

public class ResultTests
{
    [Fact]
    public void OkExposesValue()
    {
        var result = Result.Ok(42);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(42, result.Value);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void FailureThrowsWhenValueIsRead()
    {
        var result = Result.Fail<int>(Error.NotFound("Imovel nao encontrado."));

        Assert.True(result.IsFailure);
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void FailureCarriesError()
    {
        var error = Error.Conflict("Inscricao ja cadastrada.", "property.duplicated");

        var result = Result.Fail(error);

        Assert.Equal("property.duplicated", result.Error.Code);
    }

    [Fact]
    public void SuccessCannotCarryError()
    {
        Assert.Throws<InvalidOperationException>(() => Result.Fail(Error.None));
    }

    [Fact]
    public void FailureRequiresError()
    {
        Assert.Throws<InvalidOperationException>(() => Result.Fail<int>(Error.None));
    }
}