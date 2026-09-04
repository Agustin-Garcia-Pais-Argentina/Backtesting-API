using Microsoft.Extensions.Logging.Abstractions;
using PortfolioAnalytics.Api.Backtesting;
using PortfolioAnalytics.Application.Commands;
using PortfolioAnalytics.Application.DTOs;
using PortfolioAnalytics.Application.Handlers;
using PortfolioAnalytics.Application.Services;
using PortfolioAnalytics.Infrastructure.Repositories;

namespace PortfolioAnalytics.UnitTests;

public sealed class BacktestExecutionWorkerTests
{
    [Fact]
    public async Task AcceptedQueuedRun_TransitionsToCompletedWithMetrics()
    {
        var userId = Guid.NewGuid();
        var runId = Guid.NewGuid();
        var repository = new InMemoryMarketDataRepository();
        await repository.AddRangeAsync(new[]
        {
            new PortfolioAnalytics.Domain.Entities.MarketDataPoint(
                "AAPL",
                new DateOnly(2024, 1, 1),
                100m,
                102m,
                98m,
                100m,
                1000m,
                "sample"),
            new PortfolioAnalytics.Domain.Entities.MarketDataPoint(
                "AAPL",
                new DateOnly(2024, 1, 2),
                100m,
                105m,
                99m,
                103m,
                1000m,
                "sample")
        });

        var command = new RunBacktestCommand(
            userId,
            "AAPL",
            new DateOnly(2024, 1, 1),
            new DateOnly(2024, 1, 2),
            10_000m);
        var store = new BacktestExecutionStore();
        store.Save(new BacktestRunResponse
        {
            Id = runId,
            UserId = userId,
            Symbol = command.Symbol,
            Status = "Queued"
        });

        var queue = new BacktestExecutionQueue(1);
        await queue.EnqueueAsync(new BacktestWorkItem(runId, userId, command));
        var worker = new BacktestExecutionWorker(
            queue,
            new RunBacktestHandler(repository, new BacktestService()),
            store,
            NullLogger<BacktestExecutionWorker>.Instance);

        await worker.StartAsync(CancellationToken.None);
        try
        {
            BacktestRunResponse? completedRun = null;
            for (var attempt = 0; attempt < 100; attempt++)
            {
                completedRun = store.GetById(runId, userId);
                if (completedRun?.Status == "Completed")
                {
                    break;
                }

                await Task.Delay(10);
            }

            Assert.NotNull(completedRun);
            Assert.Equal("Completed", completedRun!.Status);
            Assert.Equal(1, completedRun.TradeCount);
            Assert.NotNull(completedRun.CompletedAt);
        }
        finally
        {
            await worker.StopAsync(CancellationToken.None);
        }
    }
}
