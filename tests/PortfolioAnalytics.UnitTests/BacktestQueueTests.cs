using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PortfolioAnalytics.Api.Controllers;
using PortfolioAnalytics.Application.Commands;
using PortfolioAnalytics.Application.DTOs;
using PortfolioAnalytics.Application.Services;

namespace PortfolioAnalytics.UnitTests;

public sealed class BacktestQueueTests
{
    [Fact]
    public async Task EnqueueAsync_WhenCapacityIsReached_ReturnsQueueFullError()
    {
        var queue = new BacktestExecutionQueue(1);
        await queue.EnqueueAsync(CreateWorkItem());

        var exception = await Assert.ThrowsAsync<BacktestQueueFullException>(
            () => queue.EnqueueAsync(CreateWorkItem()).AsTask());

        Assert.Equal(1, exception.Capacity);
    }

    [Fact]
    public async Task Controller_WhenRequestIsCancelled_DoesNotKeepQueuedRun()
    {
        var store = new BacktestExecutionStore();
        var queue = new BacktestExecutionQueue(1);
        var controller = CreateController(store, queue);
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            controller.RunAsync(CreateRequest(), cancellationSource.Token));

        Assert.Empty(store.GetRecent(GetUserId()));
    }

    [Fact]
    public async Task Controller_WhenQueueIsFull_ReturnsServiceUnavailable()
    {
        var store = new BacktestExecutionStore();
        var queue = new BacktestExecutionQueue(1);
        await queue.EnqueueAsync(CreateWorkItem());
        var controller = CreateController(store, queue);

        var result = await controller.RunAsync(CreateRequest(), CancellationToken.None);

        var response = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, response.StatusCode);
        Assert.Empty(store.GetRecent(GetUserId()));
    }

    [Fact]
    public async Task Controller_WhenQueueAcceptsRequest_ReturnsQueuedRun()
    {
        var store = new BacktestExecutionStore();
        var queue = new BacktestExecutionQueue(1);
        var controller = CreateController(store, queue);

        var result = await controller.RunAsync(CreateRequest(), CancellationToken.None);

        var response = Assert.IsType<AcceptedAtRouteResult>(result.Result);
        var run = Assert.IsType<BacktestRunResponse>(response.Value);
        Assert.Equal("Queued", run.Status);
        Assert.Same(run, store.GetById(run.Id, GetUserId()));

        using var readCancellation = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        var queuedItem = await ReadSingleAsync(queue, readCancellation.Token);
        Assert.Equal(run.Id, queuedItem.RunId);
    }

    private static BacktestsController CreateController(BacktestExecutionStore store, BacktestExecutionQueue queue)
    {
        var controller = new BacktestsController(store, queue)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, GetUserId().ToString())
                    }, "test"))
                }
            }
        };

        return controller;
    }

    private static BacktestRunRequest CreateRequest()
    {
        return new BacktestRunRequest
        {
            Symbol = "AAPL",
            StartDate = new DateOnly(2024, 1, 1),
            EndDate = new DateOnly(2024, 1, 2),
            InitialCapital = 10_000m
        };
    }

    private static BacktestWorkItem CreateWorkItem()
    {
        var userId = GetUserId();
        return new BacktestWorkItem(
            Guid.NewGuid(),
            userId,
            new RunBacktestCommand(
                userId,
                "AAPL",
                new DateOnly(2024, 1, 1),
                new DateOnly(2024, 1, 2),
                10_000m));
    }

    private static Guid GetUserId()
    {
        return Guid.Parse("9c6d9f18-0fa7-4a8f-a50b-87ea5c9fd9b2");
    }

    private static async Task<BacktestWorkItem> ReadSingleAsync(
        BacktestExecutionQueue queue,
        CancellationToken cancellationToken)
    {
        await foreach (var item in queue.ReadAllAsync(cancellationToken))
        {
            return item;
        }

        throw new InvalidOperationException("The queue completed before returning a work item.");
    }
}
