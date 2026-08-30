using PortfolioAnalytics.Application.Commands;
using PortfolioAnalytics.Domain.Entities;
using PortfolioAnalytics.Domain.Interfaces;

namespace PortfolioAnalytics.Application.Handlers;

public sealed class AddPositionHandler
{
    private readonly IPortfolioRepository _portfolioRepository;

    public AddPositionHandler(IPortfolioRepository portfolioRepository)
    {
        _portfolioRepository = portfolioRepository;
    }

    public async Task<Portfolio> HandleAsync(AddPositionCommand command, CancellationToken cancellationToken = default)
    {
        var portfolio = await _portfolioRepository.GetByIdAsync(command.PortfolioId, cancellationToken)
            ?? throw new InvalidOperationException($"Portfolio '{command.PortfolioId}' was not found.");

        var position = new Position(
            portfolio.Id,
            command.Symbol,
            command.AssetType,
            command.Quantity,
            command.AverageCost);

        portfolio.AddPosition(position);

        await _portfolioRepository.UpdateAsync(portfolio, cancellationToken);

        return portfolio;
    }
}
