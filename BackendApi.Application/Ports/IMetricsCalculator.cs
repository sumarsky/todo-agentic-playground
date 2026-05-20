using BackendApi.Application.Models;
using BackendApi.Domain;

namespace BackendApi.Application.Ports;

public interface IMetricsCalculator
{
    IReadOnlyList<EndpointMetric> Calculate(IReadOnlyList<LogEntry> logs);
}
