using System.Net;
using System.Text.Json;
using BackendApi.Application;
using BackendApi.Application.Ports;
using BackendApi.Domain;
using BackendApi.Tests.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BackendApi.Tests;

[Collection("Sequential")]
public class LogsApiIntegrationTests : IAsyncLifetime
{
    private static TestWebApplicationFactory? _sharedFactory;
    private static FakeLogStore? _sharedLogStore;
    private HttpClient _client = null!;
    private TestWebApplicationFactory _factory = null!;
    private FakeLogStore _logStore = null!;

    public async Task InitializeAsync()
    {
        if (_sharedFactory == null)
        {
            _sharedFactory = new TestWebApplicationFactory();
            _sharedLogStore = (FakeLogStore)_sharedFactory.Services.GetRequiredService(typeof(ILogStore));
        }
        _factory = _sharedFactory;
        _client = _factory.CreateClient();
        _logStore = _sharedLogStore!;
        _logStore.Clear();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await Task.CompletedTask;
    }

    [Fact]
    public async Task GetLogs_NoParams_ReturnsAllEntriesOrderedByTimestampDesc()
    {
        // Arrange: seed log entries with different timestamps
        var baseTime = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        await _logStore.WriteAsync(new LogEntry(Guid.NewGuid(), baseTime.AddMinutes(1), "info", "api", "First log"));
        await _logStore.WriteAsync(new LogEntry(Guid.NewGuid(), baseTime.AddMinutes(3), "error", "api", "Third log"));
        await _logStore.WriteAsync(new LogEntry(Guid.NewGuid(), baseTime.AddMinutes(2), "warning", "db", "Second log"));

        // Act
        var response = await _client.GetAsync("/api/logs");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(responseBody);
        var entries = json.RootElement;

        Assert.Equal(3, entries.GetArrayLength());

        var first = entries[0];
        var second = entries[1];
        var third = entries[2];

        Assert.Equal("Third log", first.GetProperty("message").GetString());
        Assert.Equal("error", first.GetProperty("level").GetString());

        Assert.Equal("Second log", second.GetProperty("message").GetString());
        Assert.Equal("warning", second.GetProperty("level").GetString());

        Assert.Equal("First log", third.GetProperty("message").GetString());
        Assert.Equal("info", third.GetProperty("level").GetString());
    }

    [Fact]
    public async Task GetLogs_FilterByLevel_ReturnsOnlyMatchingEntries()
    {
        // Arrange
        var baseTime = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        await _logStore.WriteAsync(new LogEntry(Guid.NewGuid(), baseTime.AddMinutes(1), "info", "api", "Info log"));
        await _logStore.WriteAsync(new LogEntry(Guid.NewGuid(), baseTime.AddMinutes(2), "error", "api", "Error log"));
        await _logStore.WriteAsync(new LogEntry(Guid.NewGuid(), baseTime.AddMinutes(3), "warning", "db", "Warning log"));

        // Act
        var response = await _client.GetAsync("/api/logs?level=error");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(responseBody);
        var entries = json.RootElement;

        Assert.Single(entries.EnumerateArray());
        Assert.Equal("Error log", entries[0].GetProperty("message").GetString());
        Assert.Equal("error", entries[0].GetProperty("level").GetString());
    }

    [Fact]
    public async Task GetLogs_FilterByMessage_PartialCaseInsensitiveMatch()
    {
        // Arrange
        var baseTime = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        await _logStore.WriteAsync(new LogEntry(Guid.NewGuid(), baseTime.AddMinutes(1), "info", "api", "User created todos item"));
        await _logStore.WriteAsync(new LogEntry(Guid.NewGuid(), baseTime.AddMinutes(2), "info", "api", "Request processed"));
        await _logStore.WriteAsync(new LogEntry(Guid.NewGuid(), baseTime.AddMinutes(3), "error", "db", "Failed to save Todos"));

        // Act
        var response = await _client.GetAsync("/api/logs?message=todos");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(responseBody);
        var entries = json.RootElement;

        Assert.Equal(2, entries.GetArrayLength());
        Assert.All(entries.EnumerateArray(), e =>
            Assert.Contains("todos", e.GetProperty("message").GetString(), StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetLogs_FilterByLevelAndMessage_CombinedFiltering()
    {
        // Arrange
        var baseTime = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        await _logStore.WriteAsync(new LogEntry(Guid.NewGuid(), baseTime.AddMinutes(1), "info", "api", "User created todos"));
        await _logStore.WriteAsync(new LogEntry(Guid.NewGuid(), baseTime.AddMinutes(2), "error", "api", "Failed to load todos"));
        await _logStore.WriteAsync(new LogEntry(Guid.NewGuid(), baseTime.AddMinutes(3), "error", "db", "Connection timeout"));

        // Act
        var response = await _client.GetAsync("/api/logs?level=error&message=todos");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(responseBody);
        var entries = json.RootElement;

        Assert.Single(entries.EnumerateArray());
        Assert.Equal("Failed to load todos", entries[0].GetProperty("message").GetString());
        Assert.Equal("error", entries[0].GetProperty("level").GetString());
    }

    [Fact]
    public async Task GetLogs_NoEntries_ReturnsEmptyArray()
    {
        // Act
        var response = await _client.GetAsync("/api/logs");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(responseBody);
        var entries = json.RootElement;

        Assert.Equal(0, entries.GetArrayLength());
    }
}
