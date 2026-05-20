using BackendApi.Application;
using BackendApi.Application.Ports;
using BackendApi.Application.Services;
using BackendApi.Application.UseCases;
using BackendApi.Domain;
using BackendApi.Tests.TestDoubles;
using Xunit;

namespace BackendApi.Tests.Application.UseCases;

public class CalculateMetricsUseCaseTests
{
    [Fact]
    public async Task Execute_WithLogs_ReturnsAggregatedMetrics()
    {
        // Arrange
        var logStore = new FakeLogStore();
        var calculator = new DefaultMetricsCalculator();
        var useCase = new CalculateMetricsUseCase(logStore, calculator);

        var now = DateTime.UtcNow;
        await logStore.WriteAsync(new LogEntry(Guid.NewGuid(), now.AddMinutes(-5), "INFO", "test", "msg", "GET", "/api/todos", 200, 50.0, null, null, null));
        await logStore.WriteAsync(new LogEntry(Guid.NewGuid(), now.AddMinutes(-3), "INFO", "test", "msg", "GET", "/api/todos", 500, 150.0, null, null, null));
        await logStore.WriteAsync(new LogEntry(Guid.NewGuid(), now.AddMinutes(-1), "INFO", "test", "msg", "POST", "/api/todos", 201, 75.0, null, null, null));

        // Act
        var result = await useCase.Execute(TimeSpan.FromMinutes(10));

        // Assert
        Assert.NotEmpty(result);
        var getMetric = result.Single(m => m.Endpoint == "GET /api/todos");
        Assert.Equal(2, getMetric.TotalRequests);
        Assert.Equal(1, getMetric.ErrorCount);
        Assert.Equal(100.0, getMetric.AverageLatencyMs);
    }

    [Fact]
    public async Task Execute_WithCancelledToken_PropagatesCancellation()
    {
        // Arrange
        var logStore = new CancellingLogStore();
        var calculator = new DefaultMetricsCalculator();
        var useCase = new CalculateMetricsUseCase(logStore, calculator);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => useCase.Execute(TimeSpan.FromMinutes(10), cts.Token));
    }

    [Fact]
    public async Task Execute_WithNoLogs_ReturnsEmptyMetrics()
    {
        // Arrange
        var logStore = new FakeLogStore();
        var calculator = new DefaultMetricsCalculator();
        var useCase = new CalculateMetricsUseCase(logStore, calculator);

        // Act
        var result = await useCase.Execute(TimeSpan.FromMinutes(10));

        // Assert
        Assert.Empty(result);
    }

    private class CancellingLogStore : ILogStore
    {
        public Task WriteAsync(LogEntry entry, CancellationToken ct = default) => Task.CompletedTask;

        public Task<IReadOnlyList<LogEntry>> QueryAsync(LogFilter filter, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            return Task.FromResult<IReadOnlyList<LogEntry>>(Array.Empty<LogEntry>());
        }
    }
}
