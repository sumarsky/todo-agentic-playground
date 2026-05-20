using BackendApi.Application.Models;
using Xunit;

namespace BackendApi.Tests.Application.Models;

public class EndpointMetricTests
{
    [Fact]
    public void EndpointMetric_WithValidData_ReturnsCorrectErrorRate()
    {
        // Arrange
        var metric = new EndpointMetric(
            Endpoint: "/api/users",
            ErrorCount: 5,
            AverageLatencyMs: 120.5,
            TotalRequests: 100);

        // Act
        var errorRate = metric.ErrorRate;

        // Assert
        Assert.Equal(0.05, errorRate);
    }

    [Fact]
    public void EndpointMetric_WithZeroTotalRequests_ReturnsZeroErrorRate()
    {
        // Arrange
        var metric = new EndpointMetric(
            Endpoint: "/api/empty",
            ErrorCount: 0,
            AverageLatencyMs: 0,
            TotalRequests: 0);

        // Act
        var errorRate = metric.ErrorRate;

        // Assert
        Assert.Equal(0, errorRate);
    }

    [Fact]
    public void EndpointMetric_WithLowErrorRate_IsHealthy()
    {
        // Arrange
        var metric = new EndpointMetric(
            Endpoint: "/api/healthy",
            ErrorCount: 2,
            AverageLatencyMs: 50,
            TotalRequests: 100);

        // Act & Assert
        Assert.True(metric.IsHealthy);
    }

    [Fact]
    public void EndpointMetric_WithHighErrorRate_IsNotHealthy()
    {
        // Arrange
        var metric = new EndpointMetric(
            Endpoint: "/api/unhealthy",
            ErrorCount: 10,
            AverageLatencyMs: 200,
            TotalRequests: 100);

        // Act & Assert
        Assert.False(metric.IsHealthy);
    }

    [Fact]
    public void EndpointMetric_AtExactlyFivePercentErrorRate_IsNotHealthy()
    {
        // Arrange
        var metric = new EndpointMetric(
            Endpoint: "/api/threshold",
            ErrorCount: 5,
            AverageLatencyMs: 100,
            TotalRequests: 100);

        // Act & Assert
        Assert.False(metric.IsHealthy);
    }
}
