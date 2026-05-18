using BackendApi.Application;
using BackendApi.Application.Ports;
using BackendApi.Logging;
using BackendApi.Tests.TestDoubles;
using Microsoft.Extensions.Logging;

namespace BackendApi.Tests;

public class PostgresLoggerProviderTests
{
    [Fact]
    public async Task Log_WritesToLogStore_ForWarningLevel()
    {
        var logStore = new FakeLogStore();
        var provider = new PostgresLoggerProvider(logStore);
        var logger = provider.CreateLogger("TestCategory");

        logger.LogWarning("Test warning message");

        var entries = await logStore.QueryAsync(new LogFilter { Level = "warning" });
        var entry = Assert.Single(entries, e => e.Source == "TestCategory");
        Assert.Equal("Test warning message", entry.Message);
    }

    [Fact]
    public async Task Log_WritesToLogStore_ForErrorLevelWithException()
    {
        var logStore = new FakeLogStore();
        var provider = new PostgresLoggerProvider(logStore);
        var logger = provider.CreateLogger("ExceptionHandler");

        var exception = new InvalidOperationException("Something went wrong");
        logger.LogError(exception, "Error occurred: {Message}", exception.Message);

        var entries = await logStore.QueryAsync(new LogFilter { Level = "error" });
        var entry = Assert.Single(entries, e => e.Source == "ExceptionHandler");
        Assert.Equal("Error occurred: Something went wrong", entry.Message);
        Assert.Equal("System.InvalidOperationException", entry.ExceptionType);
        Assert.Equal("Something went wrong", entry.ExceptionMessage);
    }
}
