namespace PortfolioAnalytics.Application.Abstractions;

/// <summary>
/// Defines the password hashing contract used by the application layer.
/// The implementation is chosen by infrastructure, while the business logic depends only on the abstraction.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}
