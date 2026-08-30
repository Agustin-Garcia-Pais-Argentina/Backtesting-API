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
/// This controller is intentionally thin.
/// It receives HTTP data, translates it to application commands, and returns DTOs.
/// It does not contain business logic; that belongs to the application and domain layers.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class PortfoliosController : ControllerBase
{
    private readonly CreatePortfolioHandler _createPortfolioHandler;
    private readonly AddPositionHandler _addPositionHandler;
    private readonly GetPortfolioByIdHandler _getPortfolioByIdHandler;

    public PortfoliosController(
        CreatePortfolioHandler createPortfolioHandler,
        AddPositionHandler addPositionHandler,
        GetPortfolioByIdHandler getPortfolioByIdHandler)
    {
        _createPortfolioHandler = createPortfolioHandler;
        _addPositionHandler = addPositionHandler;
        _getPortfolioByIdHandler = getPortfolioByIdHandler;
    }

    /// <summary>
    /// Creates a new portfolio for the authenticated user.
    /// The user identity is extracted from the JWT claim and used as the ownership reference.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PortfolioResponse>> CreateAsync([FromBody] CreatePortfolioRequest request, CancellationToken cancellationToken)
    {
        var userIdFromClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var currentUserId = !string.IsNullOrWhiteSpace(userIdFromClaim)
            ? Guid.Parse(userIdFromClaim)
            : request.UserId;

        var command = new CreatePortfolioCommand(currentUserId, request.Name);
        var portfolio = await _createPortfolioHandler.HandleAsync(command, cancellationToken);

        return Ok(ToResponse(portfolio));
    }

    /// <summary>
    /// Reads a portfolio by identifier and returns its public representation.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PortfolioResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var portfolio = await _getPortfolioByIdHandler.HandleAsync(new GetPortfolioByIdQuery(id), cancellationToken);
        if (portfolio is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(portfolio));
    }

    /// <summary>
    /// Adds a new asset position to an existing portfolio.
    /// The handler validates the business rules before writing to the aggregate.
    /// </summary>
    [HttpPost("{id:guid}/positions")]
    public async Task<ActionResult<PortfolioResponse>> AddPositionAsync(Guid id, [FromBody] AddPositionRequest request, CancellationToken cancellationToken)
    {
        var command = new AddPositionCommand(
            id,
            request.Symbol,
            request.AssetType,
            request.Quantity,
            request.AverageCost);

        var portfolio = await _addPositionHandler.HandleAsync(command, cancellationToken);
        return Ok(ToResponse(portfolio));
    }

    /// <summary>
    /// Converts the domain model into the HTTP response shape expected by the client.
    /// </summary>
    private static PortfolioResponse ToResponse(Portfolio portfolio)
    {
        var response = new PortfolioResponse
        {
            Id = portfolio.Id,
            UserId = portfolio.UserId,
            Name = portfolio.Name,
            CreatedAt = portfolio.CreatedAt,
            Positions = portfolio.Positions
                .Select(position => new PositionResponse
                {
                    Id = position.Id,
                    Symbol = position.Symbol,
                    AssetType = position.AssetType,
                    Quantity = position.Quantity,
                    AverageCost = position.AverageCost,
                    CreatedAt = position.CreatedAt
                })
                .ToList()
        };

        return response;
    }
}
