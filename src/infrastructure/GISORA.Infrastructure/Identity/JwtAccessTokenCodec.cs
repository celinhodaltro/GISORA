using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using GISORA.Application.Identity;

namespace GISORA.Infrastructure.Identity;

public sealed class JwtAccessTokenCodec : IAccessTokenCodec
{
    private const string Issuer = "gisora";
    private const string Audience = "gisora-web";
    private const string EncodedHeader = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9";

    private static readonly TimeSpan Lifetime = TimeSpan.FromHours(8);

    private static readonly byte[] SigningKey =
        SHA256.HashData(Encoding.UTF8.GetBytes("gisora-demonstracao-sessao-local"));

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public AccessToken Issue(SessionUser user, DateTimeOffset issuedAt)
    {
        var expiresAt = issuedAt.Add(Lifetime);

        var payload = new JwtPayload
        {
            Subject = user.Id.ToString(),
            Name = user.Name,
            Email = user.Email,
            Role = user.Role.ToString(),
            Issuer = Issuer,
            Audience = Audience,
            IssuedAt = issuedAt.ToUnixTimeSeconds(),
            ExpiresAt = expiresAt.ToUnixTimeSeconds(),
        };

        var body = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(payload, JsonOptions));
        var signature = Base64UrlEncode(Sign($"{EncodedHeader}.{body}"));

        return new AccessToken($"{EncodedHeader}.{body}.{signature}", expiresAt);
    }

    public AccessTokenClaims? Read(string token)
    {
        var parts = token.Split('.');

        if (parts.Length != 3 || parts[0] != EncodedHeader)
        {
            return null;
        }

        var signature = Base64UrlDecode(parts[2]);

        if (signature is null || CryptographicOperations.FixedTimeEquals(Sign($"{parts[0]}.{parts[1]}"), signature) is false)
        {
            return null;
        }

        JwtPayload? payload;

        try
        {
            var body = Base64UrlDecode(parts[1]);

            if (body is null)
            {
                return null;
            }

            payload = JsonSerializer.Deserialize<JwtPayload>(body, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }

        if (payload is null
            || payload.Issuer != Issuer
            || payload.Audience != Audience
            || Guid.TryParse(payload.Subject, out var userId) is false)
        {
            return null;
        }

        var expiresAt = DateTimeOffset.FromUnixTimeSeconds(payload.ExpiresAt);

        return expiresAt <= DateTimeOffset.UtcNow
            ? null
            : new AccessTokenClaims(userId, expiresAt);
    }

    private static byte[] Sign(string value)
        => HMACSHA256.HashData(SigningKey, Encoding.UTF8.GetBytes(value));

    private static string Base64UrlEncode(byte[] value)
        => Convert.ToBase64String(value).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static byte[]? Base64UrlDecode(string value)
    {
        var normalized = value.Replace('-', '+').Replace('_', '/');

        switch (normalized.Length % 4)
        {
            case 2:
                normalized += "==";
                break;
            case 3:
                normalized += "=";
                break;
            case 1:
                return null;
        }

        try
        {
            return Convert.FromBase64String(normalized);
        }
        catch (FormatException)
        {
            return null;
        }
    }

    private sealed record JwtPayload
    {
        [JsonPropertyName("sub")]
        public string Subject { get; init; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; init; } = string.Empty;

        [JsonPropertyName("role")]
        public string Role { get; init; } = string.Empty;

        [JsonPropertyName("iss")]
        public string Issuer { get; init; } = string.Empty;

        [JsonPropertyName("aud")]
        public string Audience { get; init; } = string.Empty;

        [JsonPropertyName("iat")]
        public long IssuedAt { get; init; }

        [JsonPropertyName("exp")]
        public long ExpiresAt { get; init; }
    }
}