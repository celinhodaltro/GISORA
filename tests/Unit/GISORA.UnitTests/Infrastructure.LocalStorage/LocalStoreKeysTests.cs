using GISORA.Domain.Common;
using GISORA.Infrastructure.Persistence;

namespace GISORA.UnitTests.Infrastructure.LocalStorage;

public class LocalStoreKeysTests
{
    [Fact]
    public void BuildsNamespacedKey()
    {
        Assert.Equal("gisora:properties:abc", LocalStoreKeys.For("properties", "abc"));
    }

    [Fact]
    public void BuildsNamespacePrefix()
    {
        Assert.Equal("gisora:properties:", LocalStoreKeys.NamespacePrefix("properties"));
    }

    [Fact]
    public void RejectsNamespaceWithSeparator()
    {
        var exception = Assert.Throws<DomainException>(() => LocalStoreKeys.For("pro:perties", "abc"));

        Assert.Equal("storage.key.invalid", exception.Code);
    }

    [Fact]
    public void RejectsEmptyKey()
    {
        Assert.Throws<DomainException>(() => LocalStoreKeys.For("properties", " "));
    }
}