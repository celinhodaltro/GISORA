using GISORA.Domain.Common;
using GISORA.Domain.Geospatial;

namespace GISORA.UnitTests.Geospatial;

public class SpatialReferenceTests
{
    [Fact]
    public void FromEpsgNormalizesAuthority()
    {
        var reference = SpatialReference.From(" epsg ", 4326);

        Assert.Equal("EPSG", reference.Authority);
        Assert.Equal(4326, reference.Code);
        Assert.Equal("EPSG:4326", reference.ToString());
    }

    [Fact]
    public void ReferencesWithSameAuthorityAndCodeAreEqual()
    {
        Assert.Equal(SpatialReference.FromEpsg(4674), SpatialReference.FromEpsg(4674));
    }

    [Fact]
    public void ReferencesWithDifferentCodesAreNotEqual()
    {
        Assert.NotEqual(SpatialReference.FromEpsg(4326), SpatialReference.FromEpsg(4674));
    }

    [Fact]
    public void CodeMustBePositive()
    {
        Assert.Throws<DomainException>(() => SpatialReference.FromEpsg(0));
    }

    [Fact]
    public void AuthorityIsRequired()
    {
        Assert.Throws<DomainException>(() => SpatialReference.From("  ", 4326));
    }
}