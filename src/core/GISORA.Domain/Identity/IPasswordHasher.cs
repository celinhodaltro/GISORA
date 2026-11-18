namespace GISORA.Domain.Identity;

public sealed record HashedPassword(string Hash, string Salt);

public interface IPasswordHasher
{
    HashedPassword Hash(string password);

    bool Verify(string password, HashedPassword hashedPassword);
}