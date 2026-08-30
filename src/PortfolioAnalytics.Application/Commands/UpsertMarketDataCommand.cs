using PortfolioAnalytics.Domain.Entities;

namespace PortfolioAnalytics.Application.Commands;

/// <summary>
/// Represents a write operation that stores or refreshes market data records for one or more symbols.
/// </summary>
public sealed record UpsertMarketDataCommand(IEnumerable<MarketDataPoint> Points);
