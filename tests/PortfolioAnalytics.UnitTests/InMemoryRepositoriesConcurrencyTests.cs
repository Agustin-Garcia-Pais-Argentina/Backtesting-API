using PortfolioAnalytics.Domain.Entities;
using PortfolioAnalytics.Domain.Enums;
using PortfolioAnalytics.Infrastructure.Repositories;

namespace PortfolioAnalytics.UnitTests;

public sealed class InMemoryRepositoriesConcurrencyTests
{
    [Fact]
    public async Task InMemoryUserRepository_ConcurrentAddsWithSameEmail_StoresOnlyOneUser()
    {
        var repository = new InMemoryUserRepository();
        var email = "concurrent@example.com";

        var outcomes = await Task.WhenAll(
            Enumerable.Range(0, 32).Select(index => Task.Run(async () =>
            {
                try
                {
                    await repository.AddAsync(new User(email, $"User {index}", "hash"));
                    return true;
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
            })));

        Assert.Single(outcomes, succeeded => succeeded);
        Assert.NotNull(await repository.GetByEmailAsync(email));
    }

    [Fact]
    public async Task InMemoryPortfolioRepository_ConcurrentPositionUpdates_PreserveAllDistinctPositions()
    {
        var repository = new InMemoryPortfolioRepository();
        var portfolio = new Portfolio(Guid.NewGuid(), "Concurrent portfolio");
        await repository.AddAsync(portfolio);

        var updateTasks = Enumerable.Range(0, 32).Select(index => Task.Run(async () =>
        {
            var loaded = await repository.GetByIdAsync(portfolio.Id);
            Assert.NotNull(loaded);

            loaded!.AddPosition(new Position(
                portfolio.Id,
                $"AST{index}",
                AssetType.Stock,
                1m,
                100m));

            await repository.UpdateAsync(loaded);
        }));

        await Task.WhenAll(updateTasks);

        var saved = await repository.GetByIdAsync(portfolio.Id);
        Assert.NotNull(saved);
        Assert.Equal(32, saved!.Positions.Count);
    }

    [Fact]
    public async Task InMemoryMarketDataRepository_ConcurrentUpserts_ReturnSafeDeduplicatedSnapshots()
    {
        var repository = new InMemoryMarketDataRepository();
        var point = new MarketDataPoint(
            "AAPL",
            new DateOnly(2024, 1, 2),
            100m,
            110m,
            95m,
            105m,
            1000m,
            "sample");

        await Task.WhenAll(
            Enumerable.Range(0, 32).Select(_ =>
                Task.Run(() => repository.AddRangeAsync(new[] { point }))));

        var snapshot = (await repository.GetBySymbolAsync(
            "aapl",
            new DateOnly(2024, 1, 1),
            new DateOnly(2024, 1, 3))).ToList();

        Assert.Single(snapshot);
        Assert.Same(point, snapshot[0]);

        await repository.AddRangeAsync(new[]
        {
            new MarketDataPoint(
                "AAPL",
                new DateOnly(2024, 1, 3),
                105m,
                115m,
                100m,
                110m,
                1000m,
                "sample")
        });

        Assert.Single(snapshot);
        Assert.Equal(2, (await repository.GetBySymbolAsync(
            "AAPL",
            new DateOnly(2024, 1, 1),
            new DateOnly(2024, 1, 3))).Count());
    }
}
