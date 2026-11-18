using GISORA.Application.Identity;
using GISORA.Domain.Identity;
using GISORA.Infrastructure.Identity;

namespace GISORA.UnitTests.Identity;

public class AccessTokenCodecTests
{
    private readonly JwtAccessTokenCodec _codec = new();

    private static SessionUser User() => new(
        Guid.CreateVersion7(),
        "Maria",
        "maria@gisora.gov.br",
        UserRole.Analyst,
        RoleRights.For(UserRole.Analyst));

    [Fact]
    public void IssueProducesThreePartTokenWithSessionExpiration()
    {
        var issuedAt = DateTimeOffset.UtcNow;

        var token = _codec.Issue(User(), issuedAt);

        Assert.Equal(3, token.Value.Split('.').Length);
        Assert.Equal(issuedAt.AddHours(8), token.ExpiresAt);
    }

    [Fact]
    public void ReadReturnsSubjectFromValidToken()
    {
        var user = User();

        var claims = _codec.Read(_codec.Issue(user, DateTimeOffset.UtcNow).Value);

        Assert.NotNull(claims);
        Assert.Equal(user.Id, claims.UserId);
    }

    [Fact]
    public void ReadRejectsTamperedSignature()
    {
        var token = _codec.Issue(User(), DateTimeOffset.UtcNow).Value;
        var parts = token.Split('.');

        var tampered = $"{parts[0]}.{parts[1]}.{parts[2][..^2]}AA";

        Assert.Null(_codec.Read(tampered));
    }

    [Fact]
    public void ReadRejectsTamperedPayload()
    {
        var token = _codec.Issue(User(), DateTimeOffset.UtcNow).Value;
        var parts = token.Split('.');

        var tampered = $"{parts[0]}.{Invert(parts[1])}.{parts[2]}";

        Assert.Null(_codec.Read(tampered));
    }

    [Fact]
    public void ReadRejectsExpiredToken()
    {
        var token = _codec.Issue(User(), DateTimeOffset.UtcNow.AddHours(-9)).Value;

        Assert.Null(_codec.Read(token));
    }

    [Fact]
    public void ReadRejectsMalformedToken()
    {
        Assert.Null(_codec.Read("nao-e-um-token"));
    }

    private static string Invert(string value)
    {
        var chars = value.ToCharArray();
        chars[0] = chars[0] == 'e' ? 'f' : 'e';

        return new string(chars);
    }
}