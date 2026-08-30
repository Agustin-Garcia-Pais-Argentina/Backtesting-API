namespace PortfolioAnalytics.Application.Queries;

/// <summary>
/// A query asks for information without mutating the state.
/// This is deliberately separated from commands to convey intent clearly.
/// </summary>
public sealed record GetPortfolioByIdQuery(Guid Id);
