using System.Diagnostics;
using BackendApi.Application.Ports;
using BackendApi.Domain;
using Microsoft.Extensions.Logging;

namespace BackendApi.Logging;

public class PostgresLogger : ILogger
{
    private readonly ILogStore _logStore;
    private readonly string _categoryName;

    public PostgresLogger(ILogStore logStore, string categoryName)
    {
        _logStore = logStore;
        _categoryName = categoryName;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var level = logLevel switch
        {
            LogLevel.Trace or LogLevel.Debug or LogLevel.Information => "info",
            LogLevel.Warning => "warning",
            LogLevel.Error or LogLevel.Critical => "error",
            _ => "info"
        };

        var message = formatter(state, exception);
        var entry = new LogEntry(Guid.NewGuid(), DateTime.UtcNow, level, _categoryName, message)
        {
            TraceId = Activity.Current?.TraceId.ToString()
        };

        if (exception != null)
        {
            entry = entry with
            {
                ExceptionType = exception.GetType().FullName,
                ExceptionMessage = exception.Message
            };
        }

        _ = _logStore.WriteAsync(entry);
    }
}
