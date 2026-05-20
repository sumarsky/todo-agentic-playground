using BackendApi.Application.Models;
using BackendApi.Application.Ports;
using BackendApi.Domain;

namespace BackendApi.Application.Services;

public class DefaultMetricsCalculator : IMetricsCalculator
{
    public IReadOnlyList<EndpointMetric> Calculate(IReadOnlyList<LogEntry> logs)
    {
        var filtered = logs.Where(log =>
            !string.IsNullOrEmpty(log.HttpMethod) &&
            !string.IsNullOrEmpty(log.HttpPath) &&
            log.HttpStatus.HasValue &&
            log.DurationMs.HasValue
        ).ToList();

        var grouped = filtered.GroupBy(log => $"{log.HttpMethod} {log.HttpPath}");

        var metrics = new List<EndpointMetric>();
        foreach (var group in grouped)
        {
            var logsInGroup = group.ToList();
            var errorCount = logsInGroup.Count(log => log.HttpStatus >= 400);
            var totalRequests = logsInGroup.Count;
            var averageLatencyMs = logsInGroup.Average(log => log.DurationMs!.Value);

            metrics.Add(new EndpointMetric(
                Endpoint: group.Key,
                ErrorCount: errorCount,
                AverageLatencyMs: averageLatencyMs,
                TotalRequests: totalRequests
            ));
        }

        return metrics;
    }
}
