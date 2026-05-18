using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using BackendApi.Application;
using BackendApi.Domain;
using BackendApi.Tests.TestDoubles;

namespace BackendApi.Tests;

public class LogIngestionIntegrationTests : IAsyncLifetime
{
    private TestWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        _factory = new TestWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    private static StringContent ToJsonContent(object obj)
    {
        var json = JsonSerializer.Serialize(obj);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    [Fact]
    public async Task GetHealth_WritesInfoLogWithMethodPathStatusDuration()
    {
        var response = await _client.GetAsync("/health");
        response.EnsureSuccessStatusCode();

        var logStore = _factory.FakeLogStore;
        var entries = await logStore.QueryAsync(new LogFilter { Level = "info" });

        var middlewareEntry = Assert.Single(entries, e => e.Source == "middleware");
        Assert.Equal("GET", middlewareEntry.HttpMethod);
        Assert.Equal("/health", middlewareEntry.HttpPath);
        Assert.Equal(200, middlewareEntry.HttpStatus);
        Assert.True(middlewareEntry.DurationMs > 0);
        Assert.Equal("GET /health returned 200", middlewareEntry.Message);
    }

    [Fact]
    public async Task PostTodoWithEmptyTitle_WritesWarningLog()
    {
        var response = await _client.PostAsync("/todos", ToJsonContent(new { Title = "" }));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var logStore = _factory.FakeLogStore;
        var entries = await logStore.QueryAsync(new LogFilter { Level = "warning" });

        var middlewareEntry = Assert.Single(entries, e => e.Source == "middleware");
        Assert.Equal("POST", middlewareEntry.HttpMethod);
        Assert.Equal("/todos", middlewareEntry.HttpPath);
        Assert.Equal(400, middlewareEntry.HttpStatus);
        Assert.Equal("POST /todos returned 400", middlewareEntry.Message);
    }

    [Fact]
    public async Task GetUnhandledError_WritesErrorLogWithTraceId()
    {
        var response = await _client.GetAsync("/error/unhandled");
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        var logStore = _factory.FakeLogStore;
        var entries = await logStore.QueryAsync(new LogFilter { Level = "error" });

        var middlewareEntry = Assert.Single(entries, e => e.Source == "middleware");
        Assert.Equal("GET", middlewareEntry.HttpMethod);
        Assert.Equal("/error/unhandled", middlewareEntry.HttpPath);
        Assert.Equal(500, middlewareEntry.HttpStatus);
        Assert.NotNull(middlewareEntry.TraceId);
        Assert.NotEmpty(middlewareEntry.TraceId);
        Assert.Equal("GET /error/unhandled returned 500", middlewareEntry.Message);
    }

    [Fact]
    public async Task UnhandledException_WritesErrorLogWithExceptionTypeAndMessage()
    {
        var response = await _client.GetAsync("/error/unhandled");
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        var logStore = _factory.FakeLogStore;
        var entries = await logStore.QueryAsync(new LogFilter { Level = "error" });

        var exceptionEntry = entries.SingleOrDefault(e => e.Source == "exception-handler");
        Assert.NotNull(exceptionEntry);
        Assert.Equal("System.Exception", exceptionEntry.ExceptionType);
        Assert.Equal("Test unhandled exception", exceptionEntry.ExceptionMessage);
    }
}
