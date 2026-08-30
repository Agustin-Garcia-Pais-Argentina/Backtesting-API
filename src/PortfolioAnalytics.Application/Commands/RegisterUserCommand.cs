namespace PortfolioAnalytics.Application.Commands;

public sealed record RegisterUserCommand(string Email, string FullName, string Password);
