using PortfolioAnalytics.Application.Commands;
using PortfolioAnalytics.Domain.Entities;
using PortfolioAnalytics.Domain.Interfaces;

namespace PortfolioAnalytics.Application.Handlers;

/// <summary>
/// Persists a batch of market data points.
/// This handler is intentionally small because the storage policy belongs to the repository and the domain rules belong to MarketDataPoint.
/// </summary>
public sealed class SyncMarketDataHandler
{
    private readonly IMarketDataRepository _marketDataRepository;

    public SyncMarketDataHandler(IMarketDataRepository marketDataRepository)
    {
        _marketDataRepository = marketDataRepository;
    }

    public async Task<IEnumerable<MarketDataPoint>> HandleAsync(UpsertMarketDataCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var points = command.Points?.ToList() ?? new List<MarketDataPoint>();
        if (points.Count == 0)
        {
            return Array.Empty<MarketDataPoint>();
        }

        await _marketDataRepository.AddRangeAsync(points, cancellationToken);
        return points;
    }
}
