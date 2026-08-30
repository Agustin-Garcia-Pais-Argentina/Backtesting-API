namespace PortfolioAnalytics.Application.Commands;

/// <summary>
/// A command expresses an intent to change the system state.
/// In this case, creating a portfolio is a write operation with business validation.
/// </summary>
public sealed record CreatePortfolioCommand(Guid UserId, string Name);
