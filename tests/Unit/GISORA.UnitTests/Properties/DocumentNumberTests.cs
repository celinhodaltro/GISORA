using GISORA.Domain.Common;
using GISORA.Domain.Properties;

namespace GISORA.UnitTests.Properties;

public class DocumentNumberTests
{
    [Theory]
    [InlineData("529.982.247-25")]
    [InlineData("11144477735")]
    public void AcceptsValidCpf(string value)
    {
        var document = DocumentNumber.Parse(value);

        Assert.Equal(DocumentType.Cpf, document.Type);
        Assert.Equal(11, document.Digits.Length);
    }

    [Theory]
    [InlineData("04.252.011/0001-10")]
    [InlineData("11222333000181")]
    public void AcceptsValidCnpj(string value)
    {
        var document = DocumentNumber.Parse(value);

        Assert.Equal(DocumentType.Cnpj, document.Type);
        Assert.Equal(14, document.Digits.Length);
    }

    [Theory]
    [InlineData("529.982.247-24")]
    [InlineData("04.252.011/0001-11")]
    public void RejectsInvalidCheckDigits(string value)
    {
        var exception = Assert.Throws<DomainException>(() => DocumentNumber.Parse(value));

        Assert.Equal("properties.document.invalid", exception.Code);
    }

    [Theory]
    [InlineData("111.111.111-11")]
    [InlineData("00000000000000")]
    public void RejectsRepeatedDigits(string value)
    {
        var exception = Assert.Throws<DomainException>(() => DocumentNumber.Parse(value));

        Assert.Equal("properties.document.repeated", exception.Code);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("123456789012")]
    public void RejectsInvalidLength(string value)
    {
        var exception = Assert.Throws<DomainException>(() => DocumentNumber.Parse(value));

        Assert.Equal("properties.document.length", exception.Code);
    }

    [Fact]
    public void ToStringAndMaskedNeverExposeFullDocument()
    {
        var document = DocumentNumber.Parse("529.982.247-25");

        Assert.Equal("***.***.***-25", document.Masked);
        Assert.Equal(document.Masked, document.ToString());
        Assert.DoesNotContain("52998224725", document.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void CnpjMaskKeepsOnlyLastTwoDigits()
    {
        var document = DocumentNumber.Parse("04.252.011/0001-10");

        Assert.Equal("**.***.***/****-10", document.Masked);
    }

    [Fact]
    public void DocumentsWithSameDigitsAreEqual()
    {
        Assert.Equal(DocumentNumber.Parse("52998224725"), DocumentNumber.Parse("529.982.247-25"));
    }
}