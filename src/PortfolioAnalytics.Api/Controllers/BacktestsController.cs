using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioAnalytics.Application.Commands;
using PortfolioAnalytics.Application.DTOs;
using PortfolioAnalytics.Application.Handlers;
using PortfolioAnalytics.Application.Services;

namespace PortfolioAnalytics.Api.Controllers;

/// <summary>
/// Exposes the backtesting workflow to clients and keeps the HTTP layer thin.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class BacktestsController : ControllerBase
{
    private readonly RunBacktestHandler _runBacktestHandler;
    private readonly BacktestExecutionStore _executionStore;

    public BacktestsController(RunBacktestHandler runBacktestHandler, BacktestExecutionStore executionStore)
    {
        _runBacktestHandler = runBacktestHandler;
        _executionStore = executionStore;
    }

    /// <summary>
    /// Executes a deterministic backtest for a symbol within a date range.
    /// </summary>
    [HttpPost("run")]
    public async Task<ActionResult<BacktestRunResponse>> RunAsync([FromBody] BacktestRunRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest("A backtest request is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Symbol))
        {
            return BadRequest("A symbol is required.");
        }

        if (request.StartDate > request.EndDate)
        {
            return BadRequest("Start date cannot be later than end date.");
        }

        if (request.InitialCapital <= 0)
        {
            return BadRequest("Initial capital must be greater than zero.");
        }

        try
        {
            var metrics = await _runBacktestHandler.HandleAsync(
                new RunBacktestCommand(request.Symbol, request.StartDate, request.EndDate, request.InitialCapital),
                cancellationToken);

            var response = new BacktestRunResponse
            {
                Id = Guid.NewGuid(),
                Symbol = request.Symbol,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                InitialCapital = request.InitialCapital,
                StrategyType = "BuyAndHold",
                Status = "Completed",
                CreatedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow,
                TotalReturn = metrics.TotalReturn,
                AnnualizedReturn = metrics.AnnualizedReturn,
                MaxDrawdown = metrics.MaxDrawdown,
                SharpeRatio = metrics.SharpeRatio,
                Volatility = metrics.Volatility,
                TradeCount = metrics.TradeCount
            };

            _executionStore.Save(response);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(ex.Message);
        }
    }

    /// <summary>
    /// Returns the most recent in-memory backtest runs for the current MVP.
    /// </summary>
    [HttpGet]
    public ActionResult<IEnumerable<BacktestRunResponse>> GetRecentAsync()
    {
        return Ok(_executionStore.GetRecent());
    }

    /// <summary>
    /// Returns a previously generated backtest result by identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    public ActionResult<BacktestRunResponse> GetByIdAsync(Guid id)
    {
        var result = _executionStore.GetById(id);
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }
}
