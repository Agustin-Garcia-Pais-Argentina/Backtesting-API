using PortfolioAnalytics.Application.Abstractions;

namespace PortfolioAnalytics.Infrastructure.Identity;

/// <summary>
/// A simple password hasher implementation for the MVP.
/// In production-grade systems, we would usually move this to a dedicated authentication library
/// and keep the password policy stricter.
/// </summary>
public sealed class BCryptPasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty.", nameof(password));

        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool Verify(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
            return false;

        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}
