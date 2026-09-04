using System.Collections.Concurrent;
using PortfolioAnalytics.Domain.Entities;
using PortfolioAnalytics.Domain.Interfaces;

namespace PortfolioAnalytics.Infrastructure.Repositories;

/// <summary>
/// Temporary in-memory market data store for the MVP.
/// It keeps the repository contract stable while we validate price series workflows before introducing PostgreSQL or a provider adapter.
/// </summary>
public sealed class InMemoryMarketDataRepository : IMarketDataRepository
{
    private readonly ConcurrentDictionary<string, MarketDataPoint> _pointsByKey = new(StringComparer.OrdinalIgnoreCase);

    public Task AddRangeAsync(IEnumerable<MarketDataPoint> points, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        foreach (var point in points ?? Enumerable.Empty<MarketDataPoint>())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var key = BuildKey(point.Symbol, point.Date, point.Source);
            _pointsByKey[key] = point;
        }

        return Task.CompletedTask;
    }

    public Task<IEnumerable<MarketDataPoint>> GetBySymbolAsync(string symbol, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var normalizedSymbol = symbol.Trim();
        var result = _pointsByKey.Values
            .ToList()
            .Where(point => point.Symbol.Equals(normalizedSymbol, StringComparison.OrdinalIgnoreCase))
            .Where(point => point.Date >= from && point.Date <= to)
            .OrderBy(point => point.Date)
            .ToList();

        return Task.FromResult<IEnumerable<MarketDataPoint>>(result);
    }

    /// <summary>
    /// Seeds a small sample set so the API is immediately useful for local experimentation.
    /// </summary>
    public void SeedSampleData()
    {
        if (!_pointsByKey.IsEmpty)
        {
            return;
        }

        var samplePoints = new[]
        {
            new MarketDataPoint("AAPL", new DateOnly(2024, 1, 2), 186.25m, 188.10m, 184.90m, 187.40m, 5600000, "sample"),
            new MarketDataPoint("AAPL", new DateOnly(2024, 1, 3), 187.40m, 189.55m, 186.80m, 188.90m, 6100000, "sample"),
            new MarketDataPoint("AAPL", new DateOnly(2024, 1, 4), 188.90m, 190.25m, 187.50m, 189.10m, 5900000, "sample"),
            new MarketDataPoint("MSFT", new DateOnly(2024, 1, 2), 423.10m, 425.80m, 421.95m, 424.60m, 4200000, "sample"),
            new MarketDataPoint("MSFT", new DateOnly(2024, 1, 3), 424.60m, 427.90m, 423.40m, 426.20m, 4600000, "sample"),
            new MarketDataPoint("MSFT", new DateOnly(2024, 1, 4), 426.20m, 428.50m, 424.80m, 427.90m, 4700000, "sample"),
            new MarketDataPoint("SPY", new DateOnly(2024, 1, 2), 478.20m, 481.10m, 477.40m, 480.80m, 82000000, "sample"),
            new MarketDataPoint("SPY", new DateOnly(2024, 1, 3), 480.80m, 482.60m, 479.10m, 481.90m, 79000000, "sample"),
            new MarketDataPoint("SPY", new DateOnly(2024, 1, 4), 481.90m, 484.30m, 480.70m, 483.50m, 81000000, "sample")
        };

        foreach (var point in samplePoints)
        {
            _pointsByKey.TryAdd(BuildKey(point.Symbol, point.Date, point.Source), point);
        }
    }

    private static string BuildKey(string symbol, DateOnly date, string source)
    {
        return $"{symbol.Trim().ToUpperInvariant()}|{date:yyyy-MM-dd}|{source.Trim()}";
    }
}
