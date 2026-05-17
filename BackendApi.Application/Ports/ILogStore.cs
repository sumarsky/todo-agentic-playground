using BackendApi.Domain;

namespace BackendApi.Application.Ports;

public interface ILogStore
{
    Task WriteAsync(LogEntry entry, CancellationToken ct = default);
    Task<IReadOnlyList<LogEntry>> QueryAsync(LogFilter filter, CancellationToken ct = default);
}
