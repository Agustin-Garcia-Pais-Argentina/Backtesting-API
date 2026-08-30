using PortfolioAnalytics.Application.Commands;
using PortfolioAnalytics.Domain.Entities;
using PortfolioAnalytics.Domain.Interfaces;

namespace PortfolioAnalytics.Application.Handlers;

/// <summary>
/// The handler is the application-layer orchestrator for one use case.
/// It decides: validate input, create a domain entity, persist it, and return the result.
/// </summary>
public sealed class CreatePortfolioHandler
{
    private readonly IPortfolioRepository _portfolioRepository;

    public CreatePortfolioHandler(IPortfolioRepository portfolioRepository)
    {
        _portfolioRepository = portfolioRepository;
    }

    public async Task<Portfolio> HandleAsync(CreatePortfolioCommand command, CancellationToken cancellationToken = default)
    {
        // The domain object validates the entity itself, while the handler coordinates the flow.
        // This keeps the business rule near the model and the orchestration near the use case.
        var portfolio = new Portfolio(command.UserId, command.Name);

        await _portfolioRepository.AddAsync(portfolio, cancellationToken);

        return portfolio;
    }
}
