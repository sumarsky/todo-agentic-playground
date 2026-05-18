using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace BackendApi.Tests.Observability;

public class ExceptionHandlerLoggingTests : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private TestWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;
    private StringWriter _consoleOutput = null!;
    private TextWriter _originalConsoleOut = null!;

    public ExceptionHandlerLoggingTests(ITestOutputHelper output)
    {
        _output = output;
    }

    public async Task InitializeAsync()
    {
        _consoleOutput = new StringWriter();
        _originalConsoleOut = Console.Out;
        Console.SetOut(_consoleOutput);

        _factory = new TestWebApplicationFactory();
        _client = _factory.CreateClient();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        Console.SetOut(_originalConsoleOut);
        _consoleOutput.Dispose();
    }

    [Fact]
    public async Task UnhandledException_LogsExceptionWithTraceContext()
    {
        // Arrange + Act: trigger unhandled exception
        var response = await _client.GetAsync("/error/unhandled");
        Assert.Equal(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);

        // Give async log exporters time to flush
        await Task.Delay(500);

        // Assert: capture console output contains structured log with trace ID and exception details
        var logOutput = _consoleOutput.ToString();
        _output.WriteLine($"Console output:\n{logOutput}");

        Assert.NotEmpty(logOutput);
        
        // Parse JSON log lines and find exception log entry from Program category
        var logLines = logOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var exceptionLog = logLines
            .Select(line => {
                try { return JsonDocument.Parse(line); }
                catch { return null; }
            })
            .FirstOrDefault(doc => 
                doc?.RootElement.TryGetProperty("Category", out var cat) == true && 
                cat.GetString() == "Program" &&
                doc?.RootElement.TryGetProperty("LogLevel", out var level) == true &&
                level.GetString() == "Error");

        Assert.NotNull(exceptionLog);
        
        var root = exceptionLog.RootElement;
        
        // Verify exception details logged
        Assert.True(root.TryGetProperty("Exception", out _));
        
        // Verify trace ID in State
        Assert.True(root.TryGetProperty("State", out var state));
        Assert.True(state.TryGetProperty("TraceId", out var traceId));
        Assert.False(string.IsNullOrEmpty(traceId.GetString()));
        
        // Verify exception type in State
        Assert.True(state.TryGetProperty("ExceptionType", out var exceptionType));
        Assert.False(string.IsNullOrEmpty(exceptionType.GetString()));
    }
}
