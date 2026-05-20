namespace BackendApi.Application.Models;

public record EndpointMetric(string Endpoint, int ErrorCount, double AverageLatencyMs, int TotalRequests)
{
    public double ErrorRate => TotalRequests == 0 ? 0 : (double)ErrorCount / TotalRequests;
    public bool IsHealthy => ErrorRate < 0.05;
}
