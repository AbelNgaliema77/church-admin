using System.Security.Cryptography;

namespace ChurchAdmin.Api.Common;

public sealed class PasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;

    public string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

        byte[] key = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize);

        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        string[] parts = passwordHash.Split('.');

        if (parts.Length != 3)
        {
            return false;
        }

        int iterations = int.Parse(parts[0]);
        byte[] salt = Convert.FromBase64String(parts[1]);
        byte[] expectedKey = Convert.FromBase64String(parts[2]);

        byte[] actualKey = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            expectedKey.Length);

        return CryptographicOperations.FixedTimeEquals(actualKey, expectedKey);
    }

    public string HashToken(string token)
    {
        byte[] tokenBytes = System.Text.Encoding.UTF8.GetBytes(token);
        byte[] hash = SHA256.HashData(tokenBytes);

        return Convert.ToBase64String(hash);
    }

    public string GenerateInviteToken()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }
}