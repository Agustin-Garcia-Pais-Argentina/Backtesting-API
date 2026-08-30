using PortfolioAnalytics.Application.Commands;
using PortfolioAnalytics.Application.Handlers;
using PortfolioAnalytics.Domain.Entities;
using PortfolioAnalytics.Domain.Enums;
using PortfolioAnalytics.Infrastructure.Identity;
using PortfolioAnalytics.Infrastructure.Repositories;

namespace PortfolioAnalytics.UnitTests;

// This suite covers the most valuable MVP behaviors: domain invariants and the key
// use cases we rely on before introducing a database or broader integration tests.
public class MvpFlowTests
{
    // The domain identity should reject invalid user data immediately before it reaches
    // storage or authentication logic.
    [Fact]
    public void User_ShouldRequireEmail()
    {
        var exception = Assert.Throws<ArgumentException>(() => new User("", "Test User", "hashed-password"));
        Assert.Equal("Email is required. (Parameter 'email')", exception.Message);
    }

    // Portfolio-level behavior must prevent duplicate symbols to keep the aggregate
    // consistent and avoid double-counting the same asset.
    [Fact]
    public void Portfolio_ShouldRejectDuplicateSymbolPositions()
    {
        var portfolio = new Portfolio(Guid.NewGuid(), "My Portfolio");
        var first = new Position(portfolio.Id, "AAPL", AssetType.Stock, 10m, 100m);
        portfolio.AddPosition(first);

        var duplicate = new Position(portfolio.Id, "aapl", AssetType.Stock, 5m, 99m);

        var exception = Assert.Throws<InvalidOperationException>(() => portfolio.AddPosition(duplicate));
        Assert.Contains("already exists", exception.Message);
    }

    // Market data is more useful when it validates its own integrity before being stored.
    [Fact]
    public void MarketDataPoint_ShouldRejectInvalidHighLowRange()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new MarketDataPoint("AAPL", new DateOnly(2024, 1, 2), 100m, 90m, 95m, 92m, 1000m, "sample"));

        Assert.Contains("High price cannot be lower than low price", exception.Message);
    }

    [Fact]
    public async Task InMemoryUserRepository_ShouldReturnUserByEmail()
    {
        var repository = new InMemoryUserRepository();
        var user = new User("user@test.com", "Test User", "hash");

        await repository.AddAsync(user);
        var result = await repository.GetByEmailAsync("USER@TEST.com");

        Assert.NotNull(result);
        Assert.Equal(user.Id, result!.Id);
        Assert.Equal("user@test.com", result.Email);
    }

    [Fact]
    public async Task RegisterUserHandler_ShouldHashPasswordAndPersistUser()
    {
        var repository = new InMemoryUserRepository();
        var hasher = new BCryptPasswordHasher();
        var handler = new RegisterUserHandler(repository, hasher);

        var result = await handler.HandleAsync(new RegisterUserCommand("new@user.com", "New User", "StrongPass123!"));

        Assert.Equal("new@user.com", result.Email);
        Assert.NotEqual("StrongPass123!", result.PasswordHash);
        Assert.True(hasher.Verify("StrongPass123!", result.PasswordHash));
    }

    [Fact]
    public async Task LoginUserHandler_ShouldRejectBadPassword()
    {
        var repository = new InMemoryUserRepository();
        var hasher = new BCryptPasswordHasher();
        var user = new User("login@user.com", "Login User", hasher.Hash("CorrectPass123!"));
        await repository.AddAsync(user);

        var handler = new LoginUserHandler(repository, hasher);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(new LoginUserCommand("login@user.com", "WrongPass")));

        Assert.Equal("Invalid credentials.", exception.Message);
    }

    [Fact]
    public async Task InMemoryMarketDataRepository_ShouldReturnSeriesWithinRange()
    {
        var repository = new InMemoryMarketDataRepository();
        var points = new[]
        {
            new MarketDataPoint("AAPL", new DateOnly(2024, 1, 2), 100m, 110m, 95m, 105m, 1000m, "sample"),
            new MarketDataPoint("AAPL", new DateOnly(2024, 1, 3), 105m, 112m, 100m, 110m, 1200m, "sample"),
            new MarketDataPoint("AAPL", new DateOnly(2024, 1, 4), 110m, 115m, 108m, 112m, 1300m, "sample"),
            new MarketDataPoint("MSFT", new DateOnly(2024, 1, 2), 300m, 310m, 295m, 305m, 2000m, "sample")
        };

        await repository.AddRangeAsync(points);
        var result = await repository.GetBySymbolAsync("aapl", new DateOnly(2024, 1, 2), new DateOnly(2024, 1, 3));

        Assert.Equal(2, result.Count());
        Assert.All(result, item => Assert.Equal("AAPL", item.Symbol));
    }
}
