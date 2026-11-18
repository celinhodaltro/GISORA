using GISORA.Infrastructure.Identity;

namespace GISORA.UnitTests.Identity;

public class PasswordHasherTests
{
    private readonly Pbkdf2PasswordHasher _hasher = new();

    [Fact]
    public void HashDoesNotStorePlainPasswordAndUsesUniqueSalt()
    {
        var first = _hasher.Hash("Gisora@2026");
        var second = _hasher.Hash("Gisora@2026");

        Assert.NotEqual("Gisora@2026", first.Hash);
        Assert.NotEqual(first.Salt, second.Salt);
        Assert.NotEqual(first.Hash, second.Hash);
    }

    [Fact]
    public void VerifyAcceptsCorrectPassword()
    {
        var hashed = _hasher.Hash("Gisora@2026");

        Assert.True(_hasher.Verify("Gisora@2026", hashed));
    }

    [Fact]
    public void VerifyRejectsWrongPassword()
    {
        var hashed = _hasher.Hash("Gisora@2026");

        Assert.False(_hasher.Verify("gisora@2026", hashed));
    }

    [Fact]
    public void VerifyRejectsMalformedHash()
    {
        Assert.False(_hasher.Verify("Gisora@2026", new GISORA.Domain.Identity.HashedPassword("nao-base64", "nao-base64")));
    }
}