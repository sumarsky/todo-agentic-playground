using BackendApi.Application.Ports;
using Microsoft.Extensions.Logging;

namespace BackendApi.Logging;

public class PostgresLoggerProvider : ILoggerProvider
{
    private readonly ILogStore _logStore;

    public PostgresLoggerProvider(ILogStore logStore)
    {
        _logStore = logStore;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new PostgresLogger(_logStore, categoryName);
    }

    public void Dispose()
    {
    }
}
