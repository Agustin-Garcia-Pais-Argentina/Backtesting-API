using PortfolioAnalytics.Domain.Entities;
using PortfolioAnalytics.Domain.Services;

namespace PortfolioAnalytics.UnitTests.Domain.Services;

public sealed class BacktestCalculatorTests
{
    private readonly BacktestCalculator _calculator = new();

    [Fact]
    public void EvaluateBuyAndHold_WithValidOhlcvData_ReturnsExpectedMetrics()
    {
        // Arrange
        var backtestRunId = Guid.NewGuid();
        var series = new[]
        {
            CreatePoint("AAPL", new DateOnly(2024, 1, 1), 100m, 100m),
            CreatePoint("AAPL", new DateOnly(2024, 4, 1), 110m, 110m),
            CreatePoint("AAPL", new DateOnly(2024, 7, 1), 90m, 90m),
            CreatePoint("AAPL", new DateOnly(2025, 1, 1), 121m, 121m)
        };

        // Act
        var result = _calculator.EvaluateBuyAndHold(
            backtestRunId,
            series,
            initialCash: 10_000m);

        // Assert
        Assert.Equal(backtestRunId, result.BacktestRunId);
        Assert.Equal(0.21m, result.TotalReturn);
        Assert.Equal(0.21m, result.AnnualizedReturn);
        Assert.Equal(2m / 11m, result.MaxDrawdown);
        Assert.True(result.Volatility > 0m);
        Assert.True(result.SharpeRatio > 0m);
        Assert.Equal(1, result.TradeCount);
    }

    [Fact]
    public void EvaluateBuyAndHold_WithUnorderedData_OrdersByDateBeforeCalculating()
    {
        // Arrange
        var backtestRunId = Guid.NewGuid();
        var orderedSeries = new[]
        {
            CreatePoint("AAPL", new DateOnly(2024, 1, 1), 100m, 100m),
            CreatePoint("AAPL", new DateOnly(2025, 1, 1), 121m, 121m)
        };

        var unorderedSeries = new[]
        {
            orderedSeries[1],
            orderedSeries[0]
        };

        // Act
        var result = _calculator.EvaluateBuyAndHold(
            backtestRunId,
            unorderedSeries);

        // Assert
        Assert.Equal(0.21m, result.TotalReturn);
        Assert.Equal(0.21m, result.AnnualizedReturn);
        Assert.Equal(0m, result.MaxDrawdown);
        Assert.Equal(0m, result.Volatility);
        Assert.Equal(0m, result.SharpeRatio);
        Assert.Equal(1, result.TradeCount);
    }

    [Fact]
    public void EvaluateBuyAndHold_WithSameInput_ReturnsDeterministicMetrics()
    {
        // Arrange
        var series = new[]
        {
            CreatePoint("AAPL", new DateOnly(2024, 1, 1), 100m, 100m),
            CreatePoint("AAPL", new DateOnly(2024, 7, 1), 110m, 110m),
            CreatePoint("AAPL", new DateOnly(2025, 1, 1), 105m, 105m)
        };

        var firstRunId = Guid.NewGuid();
        var secondRunId = Guid.NewGuid();

        // Act
        var firstResult = _calculator.EvaluateBuyAndHold(firstRunId, series);
        var secondResult = _calculator.EvaluateBuyAndHold(secondRunId, series);

        // Assert
        Assert.Equal(firstResult.TotalReturn, secondResult.TotalReturn);
        Assert.Equal(firstResult.AnnualizedReturn, secondResult.AnnualizedReturn);
        Assert.Equal(firstResult.MaxDrawdown, secondResult.MaxDrawdown);
        Assert.Equal(firstResult.SharpeRatio, secondResult.SharpeRatio);
        Assert.Equal(firstResult.Volatility, secondResult.Volatility);
        Assert.Equal(firstResult.TradeCount, secondResult.TradeCount);
    }

    [Fact]
    public void EvaluateBuyAndHold_WithEmptySeries_ThrowsArgumentException()
    {
        // Arrange
        var backtestRunId = Guid.NewGuid();
        var series = Array.Empty<MarketDataPoint>();

        // Act
        var exception = Assert.Throws<ArgumentException>(() =>
            _calculator.EvaluateBuyAndHold(backtestRunId, series));

        // Assert
        Assert.Contains("empty", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EvaluateBuyAndHold_WithNullSeries_ThrowsArgumentNullException()
    {
        // Arrange
        var backtestRunId = Guid.NewGuid();

        // Act
        var exception = Assert.Throws<ArgumentNullException>(() =>
            _calculator.EvaluateBuyAndHold(backtestRunId, null!));

        // Assert
        Assert.Equal("series", exception.ParamName);
    }

    [Fact]
    public void EvaluateBuyAndHold_WithConstantPrices_ReturnsZeroVolatilityAndZeroSharpeRatio()
    {
        // Arrange
        var backtestRunId = Guid.NewGuid();
        var series = new[]
        {
            CreatePoint("AAPL", new DateOnly(2024, 1, 1), 100m, 100m),
            CreatePoint("AAPL", new DateOnly(2024, 6, 1), 100m, 100m),
            CreatePoint("AAPL", new DateOnly(2025, 1, 1), 100m, 100m)
        };

        // Act
        var result = _calculator.EvaluateBuyAndHold(backtestRunId, series);

        // Assert
        Assert.Equal(0m, result.TotalReturn);
        Assert.Equal(0m, result.AnnualizedReturn);
        Assert.Equal(0m, result.MaxDrawdown);
        Assert.Equal(0m, result.Volatility);
        Assert.Equal(0m, result.SharpeRatio);
        Assert.Equal(1, result.TradeCount);
    }

    [Fact]
    public void EvaluateBuyAndHold_WithSinglePoint_ReturnsZeroVolatilityAndZeroSharpeRatio()
    {
        // Arrange
        var backtestRunId = Guid.NewGuid();
        var series = new[]
        {
            CreatePoint("AAPL", new DateOnly(2024, 1, 1), 100m, 100m)
        };

        // Act
        var result = _calculator.EvaluateBuyAndHold(backtestRunId, series);

        // Assert
        Assert.Equal(0m, result.TotalReturn);
        Assert.Equal(0m, result.AnnualizedReturn);
        Assert.Equal(0m, result.MaxDrawdown);
        Assert.Equal(0m, result.Volatility);
        Assert.Equal(0m, result.SharpeRatio);
        Assert.Equal(0, result.TradeCount);
    }

    [Fact]
    public void EvaluateBuyAndHold_WithDrawdownAndRecovery_ReturnsMaximumPeakToTroughDrawdown()
    {
        // Arrange
        var backtestRunId = Guid.NewGuid();
        var series = new[]
        {
            CreatePoint("AAPL", new DateOnly(2024, 1, 1), 100m, 100m),
            CreatePoint("AAPL", new DateOnly(2024, 4, 1), 150m, 150m),
            CreatePoint("AAPL", new DateOnly(2024, 7, 1), 100m, 100m),
            CreatePoint("AAPL", new DateOnly(2025, 1, 1), 160m, 160m)
        };

        // Act
        var result = _calculator.EvaluateBuyAndHold(backtestRunId, series);

        // Assert
        Assert.Equal(1m / 3m, result.MaxDrawdown);
    }

    [Fact]
    public void EvaluateBuyAndHold_WithNonPositiveInitialCash_ThrowsArgumentException()
    {
        // Arrange
        var backtestRunId = Guid.NewGuid();
        var series = new[]
        {
            CreatePoint("AAPL", new DateOnly(2024, 1, 1), 100m, 100m),
            CreatePoint("AAPL", new DateOnly(2025, 1, 1), 121m, 121m)
        };

        // Act
        var exception = Assert.Throws<ArgumentException>(() =>
            _calculator.EvaluateBuyAndHold(backtestRunId, series, initialCash: 0m));

        // Assert
        Assert.Contains("initial cash", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static MarketDataPoint CreatePoint(
        string symbol,
        DateOnly date,
        decimal open,
        decimal close)
    {
        return new MarketDataPoint(
            symbol,
            date,
            open,
            high: Math.Max(open, close),
            low: Math.Min(open, close),
            close,
            volume: 1_000_000m,
            source: "Test");
    }
}