namespace PortfolioAnalytics.Application.Queries;

/// <summary>
/// Requests the time series for a symbol inside a date range.
/// </summary>
public sealed record GetMarketDataBySymbolQuery(string Symbol, DateOnly From, DateOnly To);
