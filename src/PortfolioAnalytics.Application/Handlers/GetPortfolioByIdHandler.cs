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

    public async Task<Portfolio?> HandleAsync(GetPortfolioByIdQuery query, CancellationToken cancellationToken = default)
    {
        return await _portfolioRepository.GetByIdAsync(query.Id, cancellationToken);
    }
}
