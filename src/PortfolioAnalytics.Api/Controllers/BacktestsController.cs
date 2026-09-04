using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioAnalytics.Application.Commands;
using PortfolioAnalytics.Application.DTOs;
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
    private readonly BacktestExecutionStore _executionStore;
    private readonly BacktestExecutionQueue _executionQueue;

    public BacktestsController(
        BacktestExecutionStore executionStore,
        BacktestExecutionQueue executionQueue)
    {
        _executionStore = executionStore;
        _executionQueue = executionQueue;
    }

    /// <summary>
    /// Queues a deterministic backtest for asynchronous execution.
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

        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var response = new BacktestRunResponse
        {
            Id = Guid.NewGuid(),
            UserId = currentUserId,
            Symbol = request.Symbol,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            InitialCapital = request.InitialCapital,
            StrategyType = "BuyAndHold",
            Status = "Queued",
            CreatedAt = DateTime.UtcNow,
        };

        _executionStore.Save(response);
        await _executionQueue.EnqueueAsync(
            new BacktestWorkItem(
                response.Id,
                currentUserId,
                new RunBacktestCommand(currentUserId, request.Symbol, request.StartDate, request.EndDate, request.InitialCapital)),
            CancellationToken.None);

        return AcceptedAtRoute("GetBacktestById", new { id = response.Id }, response);
    }

    /// <summary>
    /// Returns the most recent in-memory backtest runs for the current MVP.
    /// </summary>
    [HttpGet]
    public ActionResult<IEnumerable<BacktestRunResponse>> GetRecentAsync()
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        return Ok(_executionStore.GetRecent(currentUserId));
    }

    /// <summary>
    /// Returns a previously generated backtest result by identifier.
    /// </summary>
    [HttpGet("{id:guid}", Name = "GetBacktestById")]
    public ActionResult<BacktestRunResponse> GetByIdAsync(Guid id)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var result = _executionStore.GetById(id, currentUserId);
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        return Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId)
            && userId != Guid.Empty;
    }
}
