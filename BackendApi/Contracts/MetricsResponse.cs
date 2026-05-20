namespace BackendApi.Contracts;

public record MetricsResponse(IReadOnlyList<EndpointMetric> Metrics);
