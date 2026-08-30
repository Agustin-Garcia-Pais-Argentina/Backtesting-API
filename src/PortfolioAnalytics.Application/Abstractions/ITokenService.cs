using PortfolioAnalytics.Domain.Entities;

namespace PortfolioAnalytics.Application.Abstractions;

/// <summary>
/// Abstracts token generation so the application layer does not depend on a specific technology.
/// The infrastructure decides whether tokens are JWT, session-based, or any other mechanism.
/// </summary>
public interface ITokenService
{
    string GenerateToken(User user);
}
