using GISORA.Application.Geospatial;
using GISORA.Domain.Common;
using GISORA.Domain.Geospatial;

namespace GISORA.UnitTests.Geospatial;

public class CrsCatalogTests
{
    [Fact]
    public void ResolvesWgs84AndSirgasUtm25S()
    {
        Assert.Equal(SpatialReference.Wgs84, CrsCatalog.Resolve("EPSG:4326"));
        Assert.Equal(SpatialReference.FromEpsg(31985), CrsCatalog.Resolve("epsg:31985"));
    }

    [Fact]
    public void RejectsUnsupportedCodeWithClearMessage()
    {
        var exception = Assert.Throws<DomainException>(() => CrsCatalog.Resolve("EPSG:9999"));

        Assert.Contains("EPSG:9999", exception.Message, StringComparison.Ordinal);
        Assert.Equal("geospatial.crs.invalid", exception.Code);
    }

    [Fact]
    public void RejectsMalformedIdentifier()
    {
        var exception = Assert.Throws<DomainException>(() => CrsCatalog.Resolve("UTM 25S"));

        Assert.Contains("EPSG:<codigo>", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RejectsEmptySelection()
    {
        Assert.False(CrsCatalog.TryResolve(null, out _, out var error));
        Assert.NotNull(error);
    }
}