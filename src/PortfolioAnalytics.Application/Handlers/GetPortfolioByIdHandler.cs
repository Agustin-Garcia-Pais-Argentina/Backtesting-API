using PortfolioAnalytics.Application.Queries;
using PortfolioAnalytics.Domain.Entities;
using PortfolioAnalytics.Domain.Interfaces;

namespace PortfolioAnalytics.Application.Handlers;

public sealed class GetPortfolioByIdHandler
{
    private readonly IPortfolioRepository _portfolioRepository;

    public GetPortfolioByIdHandler(IPortfolioRepository portfolioRepository)
    {
        _portfolioRepository = portfolioRepository;
    }

    public async Task<Portfolio?> HandleAsync(
        GetPortfolioByIdQuery query,
        Guid currentUserId,
        CancellationToken cancellationToken = default)
    {
        var portfolio = await _portfolioRepository.GetByIdAsync(query.Id, cancellationToken);
        return portfolio?.UserId == currentUserId ? portfolio : null;
    }
}
