using GISORA.Application.Territory;

namespace GISORA.UnitTests.Territory;

public class MunicipalitySearchTests
{
    private static readonly MunicipalityOption[] Municipalities =
    [
        new(2610509, "Passira", "PE"),
        new(2600104, "Afogados da Ingazeira", "PE"),
        new(2611606, "Recife", "PE"),
        new(3550308, "São Paulo", "SP"),
    ];

    [Fact]
    public void NormalizeRemovesAccentsAndCase()
    {
        Assert.Equal("sao paulo", MunicipalitySearch.Normalize("  São Paulo "));
    }

    [Fact]
    public void FilterMatchesPartialNameIgnoringCaseAndAccents()
    {
        var result = MunicipalitySearch.Filter(Municipalities, "pass");

        Assert.Single(result);
        Assert.Equal("Passira", result[0].Name);
    }

    [Fact]
    public void FilterMatchesAccentedNameWithoutAccent()
    {
        var result = MunicipalitySearch.Filter(Municipalities, "sao");

        Assert.Single(result);
        Assert.Equal("São Paulo", result[0].Name);
    }

    [Fact]
    public void FilterPrioritizesPrefixMatches()
    {
        var result = MunicipalitySearch.Filter(Municipalities, "re");

        Assert.Equal("Recife", result[0].Name);
    }

    [Fact]
    public void EmptyTermReturnsNothing()
    {
        Assert.Empty(MunicipalitySearch.Filter(Municipalities, "   "));
    }

    [Fact]
    public void FilterRespectsTakeLimit()
    {
        var result = MunicipalitySearch.Filter(Municipalities, "a", take: 2);

        Assert.True(result.Count <= 2);
    }
}