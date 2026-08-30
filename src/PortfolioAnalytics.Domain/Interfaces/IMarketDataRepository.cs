using PortfolioAnalytics.Domain.Entities;

namespace PortfolioAnalytics.Domain.Interfaces;

/// <summary>
/// Defines the persistence contract for historical market price data.
/// The implementation may use memory, relational storage, or an external provider in the future.
/// </summary>
public interface IMarketDataRepository
{
    Task AddRangeAsync(IEnumerable<MarketDataPoint> points, CancellationToken cancellationToken = default);
    Task<IEnumerable<MarketDataPoint>> GetBySymbolAsync(string symbol, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
}
