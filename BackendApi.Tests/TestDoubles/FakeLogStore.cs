using BackendApi.Application;
using BackendApi.Application.Ports;
using BackendApi.Domain;

namespace BackendApi.Tests.TestDoubles;

public class FakeLogStore : ILogStore
{
    private readonly List<LogEntry> _entries = new();
    private readonly object _lock = new();

    public Task WriteAsync(LogEntry entry, CancellationToken ct = default)
    {
        lock (_lock)
        {
            _entries.Add(entry);
        }
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<LogEntry>> QueryAsync(LogFilter filter, CancellationToken ct = default)
    {
        lock (_lock)
        {
            var entries = _entries.ToList();

            if (!string.IsNullOrWhiteSpace(filter.Level))
            {
                entries = entries.Where(e => e.Level == filter.Level).ToList();
            }

            if (!string.IsNullOrWhiteSpace(filter.Message))
            {
                entries = entries.Where(e => e.Message.Contains(filter.Message!, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (filter.Since.HasValue)
            {
                entries = entries.Where(e => e.Timestamp >= filter.Since.Value.UtcDateTime).ToList();
            }

            entries = entries.OrderByDescending(e => e.Timestamp).ToList();

            return Task.FromResult<IReadOnlyList<LogEntry>>(entries.AsReadOnly());
        }
    }
}
