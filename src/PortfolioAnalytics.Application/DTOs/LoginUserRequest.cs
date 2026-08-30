namespace PortfolioAnalytics.Application.DTOs;

public sealed class LoginUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
