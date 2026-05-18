using System.Net;
using System.Text.Json;
using Xunit;

namespace BackendApi.Tests;

public class ApiIntegrationTests : IAsyncLifetime
{
    private HttpClient _client = null!;
    private TestWebApplicationFactory _factory = null!;

    public async Task InitializeAsync()
    {
        _factory = new TestWebApplicationFactory();
        _client = _factory.CreateClient();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task UnhandledExceptionReturnsJson500Error()
    {
        // Arrange: endpoint that throws an unhandled exception
        var response = await _client.GetAsync("/error/unhandled");

        // Assert: should return 500 with JSON error response
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.TryGetProperty("error", out _));
    }

    [Fact]
    public async Task InvalidRouteReturnsJson404Error()
    {
        // Arrange: request a non-existent endpoint
        var response = await _client.GetAsync("/api/nonexistent");

        // Assert: should return 404 with JSON error response
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.TryGetProperty("error", out _));
    }

    [Fact]
    public async Task HealthEndpointReturns200()
    {
        // Arrange: request health endpoint
        var response = await _client.GetAsync("/health");

        // Assert: should return 200 OK
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.TryGetProperty("status", out var status));
        Assert.Equal("healthy", status.GetString());
    }

    [Fact]
    public async Task CorsHeadersAllowLocalhostPort3000()
    {
        // Arrange: create request with Origin header for localhost:3000
        var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("Origin", "http://localhost:3000");

        // Act
        var response = await _client.SendAsync(request);

        // Assert: should have CORS headers allowing the origin
        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
        var corsHeader = response.Headers.GetValues("Access-Control-Allow-Origin").FirstOrDefault();
        Assert.Equal("http://localhost:3000", corsHeader);
    }
}
