using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioAnalytics.Application.Commands;
using PortfolioAnalytics.Application.DTOs;
using PortfolioAnalytics.Application.Handlers;
using PortfolioAnalytics.Application.Queries;
using PortfolioAnalytics.Domain.Entities;

namespace PortfolioAnalytics.Api.Controllers;

/// <summary>
/// Exposes the market data endpoints used to load and query price series for the MVP.
/// </summary>
[Authorize]
[ApiController]
[Route("api/market-data")]
public sealed class MarketDataController : ControllerBase
{
    private readonly SyncMarketDataHandler _syncMarketDataHandler;
    private readonly GetMarketDataBySymbolHandler _getMarketDataBySymbolHandler;

    public MarketDataController(
        SyncMarketDataHandler syncMarketDataHandler,
        GetMarketDataBySymbolHandler getMarketDataBySymbolHandler)
    {
        _syncMarketDataHandler = syncMarketDataHandler;
        _getMarketDataBySymbolHandler = getMarketDataBySymbolHandler;
    }

    /// <summary>
    /// Stores a batch of market data points. This is useful for ingesting a local CSV or a small provider response.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<IEnumerable<MarketDataPointResponse>>> SyncAsync([FromBody] IEnumerable<MarketDataPointRequest> request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest("A market data payload is required.");
        }

        var points = request
            .Select(item => new MarketDataPoint(
                item.Symbol,
                item.Date,
                item.Open,
                item.High,
                item.Low,
                item.Close,
                item.Volume,
                item.Source))
            .ToList();

        var savedPoints = await _syncMarketDataHandler.HandleAsync(new UpsertMarketDataCommand(points), cancellationToken);
        return Ok(savedPoints.Select(ToResponse));
    }

    /// <summary>
    /// Returns the price history for one symbol between two dates.
    /// </summary>
    [HttpGet("{symbol}")]
    public async Task<ActionResult<IEnumerable<MarketDataPointResponse>>> GetBySymbolAsync(
        string symbol,
        [FromQuery] string from,
        [FromQuery] string to,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            return BadRequest("A symbol is required.");
        }

        if (!DateOnly.TryParse(from, out var fromDate) || !DateOnly.TryParse(to, out var toDate))
        {
            return BadRequest("The from and to query parameters must be valid ISO dates (yyyy-MM-dd).");
        }

        var result = await _getMarketDataBySymbolHandler.HandleAsync(
            new GetMarketDataBySymbolQuery(symbol, fromDate, toDate),
            cancellationToken);

        return Ok(result.Select(ToResponse));
    }

    private static MarketDataPointResponse ToResponse(MarketDataPoint point)
    {
        return new MarketDataPointResponse
        {
            Symbol = point.Symbol,
            Date = point.Date,
            Open = point.Open,
            High = point.High,
            Low = point.Low,
            Close = point.Close,
            Volume = point.Volume,
            Source = point.Source
        };
    }
}
