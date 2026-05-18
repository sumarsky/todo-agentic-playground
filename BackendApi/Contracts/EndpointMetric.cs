namespace BackendApi.Contracts;

public record EndpointMetric(string Endpoint, int FailureCount, double AvgDurationMs, int TotalRequests);
