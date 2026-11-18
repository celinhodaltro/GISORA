using System.Security.Cryptography;

using GISORA.Domain.Identity;

namespace GISORA.Infrastructure.Identity;

public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int Iterations = 10_000;
    private const int SaltSize = 16;
    private const int KeySize = 32;

    public HashedPassword Hash(string password)
    {
        ArgumentNullException.ThrowIfNull(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);

        return new HashedPassword(Convert.ToBase64String(key), Convert.ToBase64String(salt));
    }

    public bool Verify(string password, HashedPassword hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(hashedPassword.Hash) || string.IsNullOrWhiteSpace(hashedPassword.Salt))
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(hashedPassword.Salt);
            var expected = Convert.FromBase64String(hashedPassword.Hash);
            var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, expected.Length);

            return CryptographicOperations.FixedTimeEquals(expected, actual);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}