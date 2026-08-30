using PortfolioAnalytics.Application.Queries;
using PortfolioAnalytics.Domain.Entities;
using PortfolioAnalytics.Domain.Interfaces;

namespace PortfolioAnalytics.Application.Handlers;

/// <summary>
/// Retrieves a historical price series for one symbol in a time window.
/// </summary>
public sealed class GetMarketDataBySymbolHandler
{
    private readonly IMarketDataRepository _marketDataRepository;

    public GetMarketDataBySymbolHandler(IMarketDataRepository marketDataRepository)
    {
        _marketDataRepository = marketDataRepository;
    }

    public async Task<IEnumerable<MarketDataPoint>> HandleAsync(GetMarketDataBySymbolQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        return await _marketDataRepository.GetBySymbolAsync(query.Symbol, query.From, query.To, cancellationToken);
    }
}
