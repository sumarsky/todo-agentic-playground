using System.Net;
using System.Text.Json;
using BackendApi.Application.Ports;
using BackendApi.Domain;
using BackendApi.Tests.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BackendApi.Tests;

public class DashboardMetricsApiIntegrationTests : IAsyncLifetime
{
    private HttpClient _client = null!;
    private TestWebApplicationFactory _factory = null!;
    private FakeLogStore _logStore = null!;

    public async Task InitializeAsync()
    {
        _factory = new TestWebApplicationFactory();
        _client = _factory.CreateClient();
        _logStore = (FakeLogStore)_factory.Services.GetRequiredService<ILogStore>();
        _logStore.Clear();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task GetMetrics_EmptyStore_ReturnsEmptyArray()
    {
        var response = await _client.GetAsync("/api/dashboard/metrics");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.ValueKind == JsonValueKind.Array);
        Assert.Empty(json.RootElement.EnumerateArray());
    }

    [Fact]
    public async Task GetMetrics_WithEntries_ReturnsAggregatedMetrics()
    {
        var now = DateTime.UtcNow;
        await SeedEntry("POST", "/todos", 201, 30, now.AddMinutes(-10));
        await SeedEntry("POST", "/todos", 201, 50, now.AddMinutes(-20));
        await SeedEntry("GET", "/todos", 200, 20, now.AddMinutes(-30));
        await SeedEntry("POST", "/todos", 500, 100, now.AddMinutes(-40));

        var response = await _client.GetAsync("/api/dashboard/metrics?window=24h");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        var metrics = json.RootElement.EnumerateArray().ToList();
        Assert.Equal(2, metrics.Count);

        var postTodos = metrics.First(m => m.GetProperty("endpoint").GetString() == "POST /todos");
        Assert.Equal(3, postTodos.GetProperty("totalRequests").GetInt32());
        Assert.Equal(1, postTodos.GetProperty("failureCount").GetInt32());
        Assert.Equal(60, postTodos.GetProperty("avgDurationMs").GetDouble());

        var getTodos = metrics.First(m => m.GetProperty("endpoint").GetString() == "GET /todos");
        Assert.Equal(1, getTodos.GetProperty("totalRequests").GetInt32());
        Assert.Equal(0, getTodos.GetProperty("failureCount").GetInt32());
        Assert.Equal(20, getTodos.GetProperty("avgDurationMs").GetDouble());
    }

    private async Task SeedEntry(string method, string path, int status, double duration, DateTime timestamp)
    {
        var entry = new LogEntry(Guid.NewGuid(), timestamp, "INFO", "test", "test message")
        {
            HttpMethod = method,
            HttpPath = path,
            HttpStatus = status,
            DurationMs = duration
        };
        await _logStore.WriteAsync(entry);
    }

    [Fact]
    public async Task GetMetrics_FailureCount_Includes4xxAnd5xx()
    {
        var now = DateTime.UtcNow;
        await SeedEntry("POST", "/todos", 200, 10, now.AddMinutes(-10));
        await SeedEntry("POST", "/todos", 400, 10, now.AddMinutes(-15));
        await SeedEntry("POST", "/todos", 404, 10, now.AddMinutes(-20));
        await SeedEntry("POST", "/todos", 500, 10, now.AddMinutes(-25));
        await SeedEntry("POST", "/todos", 503, 10, now.AddMinutes(-30));

        var response = await _client.GetAsync("/api/dashboard/metrics?window=24h");

        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var metric = json.RootElement.EnumerateArray().Single();
        Assert.Equal(4, metric.GetProperty("failureCount").GetInt32());
        Assert.Equal(5, metric.GetProperty("totalRequests").GetInt32());
    }

    [Fact]
    public async Task GetMetrics_AvgDuration_CalculatedCorrectly()
    {
        var now = DateTime.UtcNow;
        await SeedEntry("GET", "/users", 200, 10.5, now.AddMinutes(-5));
        await SeedEntry("GET", "/users", 200, 20.3, now.AddMinutes(-10));
        await SeedEntry("GET", "/users", 200, 15.2, now.AddMinutes(-15));

        var response = await _client.GetAsync("/api/dashboard/metrics?window=24h");

        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var metric = json.RootElement.EnumerateArray().Single();
        Assert.Equal(15.333333333333334, metric.GetProperty("avgDurationMs").GetDouble());
    }

    [Fact]
    public async Task GetMetrics_Window1h_WithOldEntries_ReturnsEmptyArray()
    {
        var old = DateTime.UtcNow.AddHours(-2);
        await SeedEntry("POST", "/todos", 200, 30, old);
        await SeedEntry("POST", "/todos", 500, 50, old.AddMinutes(-30));

        var response = await _client.GetAsync("/api/dashboard/metrics?window=1h");

        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Empty(json.RootElement.EnumerateArray());
    }

    [Fact]
    public async Task GetMetrics_DefaultWindow_Is24h()
    {
        var now = DateTime.UtcNow;
        await SeedEntry("GET", "/health", 200, 5, now.AddMinutes(-30));

        var response = await _client.GetAsync("/api/dashboard/metrics");

        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Single(json.RootElement.EnumerateArray());
    }
}
