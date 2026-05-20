using BackendApi.Application.Ports;
using BackendApi.Application.Models;

namespace BackendApi.Application.UseCases;

public class CalculateMetricsUseCase
{
    private readonly ILogStore _logStore;
    private readonly IMetricsCalculator _calculator;

    public CalculateMetricsUseCase(ILogStore logStore, IMetricsCalculator calculator)
    {
        _logStore = logStore;
        _calculator = calculator;
    }

    public async Task<IReadOnlyList<EndpointMetric>> Execute(TimeSpan window, CancellationToken ct = default)
    {
        var since = DateTime.UtcNow - window;
        var logs = await _logStore.QueryAsync(new LogFilter { Since = since }, ct);
        return _calculator.Calculate(logs);
    }
}
