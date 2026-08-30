namespace PortfolioAnalytics.Application.DTOs;

/// <summary>
/// Represents the payload a client sends when creating a portfolio.
/// We keep this as a simple DTO because the API should not leak the domain model.
/// </summary>
public sealed class CreatePortfolioRequest
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
}
