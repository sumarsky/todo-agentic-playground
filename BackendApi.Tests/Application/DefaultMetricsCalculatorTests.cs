using BackendApi.Application.Ports;
using BackendApi.Application.Services;
using BackendApi.Domain;
using Xunit;

namespace BackendApi.Tests.Application;

public class DefaultMetricsCalculatorTests
{
    [Fact]
    public void Calculate_EmptyLogs_ReturnsEmptyList()
    {
        // Arrange
        IMetricsCalculator calculator = new DefaultMetricsCalculator();
        var logs = Array.Empty<LogEntry>();

        // Act
        var result = calculator.Calculate(logs);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Calculate_LogsWithMissingHttpFields_FiltersThemOut()
    {
        // Arrange
        IMetricsCalculator calculator = new DefaultMetricsCalculator();
        var logs = new List<LogEntry>
        {
            // Missing HttpMethod
            new LogEntry(Guid.NewGuid(), DateTime.UtcNow, "INFO", "test", "log1")
            {
                HttpPath = "/api/test",
                HttpStatus = 200,
                DurationMs = 50.0
            },
            // Missing HttpPath
            new LogEntry(Guid.NewGuid(), DateTime.UtcNow, "INFO", "test", "log2")
            {
                HttpMethod = "GET",
                HttpStatus = 200,
                DurationMs = 50.0
            },
            // Missing HttpStatus
            new LogEntry(Guid.NewGuid(), DateTime.UtcNow, "INFO", "test", "log3")
            {
                HttpMethod = "GET",
                HttpPath = "/api/test",
                DurationMs = 50.0
            },
            // Missing DurationMs
            new LogEntry(Guid.NewGuid(), DateTime.UtcNow, "INFO", "test", "log4")
            {
                HttpMethod = "GET",
                HttpPath = "/api/test",
                HttpStatus = 200
            },
            // Complete log - should be included
            new LogEntry(Guid.NewGuid(), DateTime.UtcNow, "INFO", "test", "log5")
            {
                HttpMethod = "GET",
                HttpPath = "/api/test",
                HttpStatus = 200,
                DurationMs = 50.0
            }
        };

        // Act
        var result = calculator.Calculate(logs);

        // Assert
        Assert.Single(result);
        Assert.Equal("GET /api/test", result[0].Endpoint);
    }

    [Fact]
    public void Calculate_LogsWithAllHttpFields_ReturnsGroupedMetrics()
    {
        // Arrange
        IMetricsCalculator calculator = new DefaultMetricsCalculator();
        var logs = new List<LogEntry>
        {
            new LogEntry(Guid.NewGuid(), DateTime.UtcNow, "INFO", "test", "log1")
            {
                HttpMethod = "GET",
                HttpPath = "/api/todos",
                HttpStatus = 200,
                DurationMs = 100.0
            },
            new LogEntry(Guid.NewGuid(), DateTime.UtcNow, "INFO", "test", "log2")
            {
                HttpMethod = "GET",
                HttpPath = "/api/todos",
                HttpStatus = 200,
                DurationMs = 200.0
            },
            new LogEntry(Guid.NewGuid(), DateTime.UtcNow, "INFO", "test", "log3")
            {
                HttpMethod = "POST",
                HttpPath = "/api/todos",
                HttpStatus = 201,
                DurationMs = 150.0
            }
        };

        // Act
        var result = calculator.Calculate(logs);

        // Assert
        Assert.Equal(2, result.Count);
        
        var getMetric = result.First(m => m.Endpoint == "GET /api/todos");
        Assert.Equal(2, getMetric.TotalRequests);
        Assert.Equal(150.0, getMetric.AverageLatencyMs);
        Assert.Equal(0, getMetric.ErrorCount);

        var postMetric = result.First(m => m.Endpoint == "POST /api/todos");
        Assert.Equal(1, postMetric.TotalRequests);
        Assert.Equal(150.0, postMetric.AverageLatencyMs);
        Assert.Equal(0, postMetric.ErrorCount);
    }

    [Fact]
    public void Calculate_LogsWithErrors_CountsErrorsCorrectly()
    {
        // Arrange
        IMetricsCalculator calculator = new DefaultMetricsCalculator();
        var logs = new List<LogEntry>
        {
            new LogEntry(Guid.NewGuid(), DateTime.UtcNow, "INFO", "test", "log1")
            {
                HttpMethod = "GET",
                HttpPath = "/api/todos",
                HttpStatus = 200,
                DurationMs = 100.0
            },
            new LogEntry(Guid.NewGuid(), DateTime.UtcNow, "ERROR", "test", "log2")
            {
                HttpMethod = "GET",
                HttpPath = "/api/todos",
                HttpStatus = 500,
                DurationMs = 200.0
            },
            new LogEntry(Guid.NewGuid(), DateTime.UtcNow, "WARN", "test", "log3")
            {
                HttpMethod = "GET",
                HttpPath = "/api/todos",
                HttpStatus = 404,
                DurationMs = 150.0
            }
        };

        // Act
        var result = calculator.Calculate(logs);

        // Assert
        Assert.Single(result);
        var metric = result[0];
        Assert.Equal("GET /api/todos", metric.Endpoint);
        Assert.Equal(3, metric.TotalRequests);
        Assert.Equal(2, metric.ErrorCount);
        Assert.Equal(150.0, metric.AverageLatencyMs);
    }

    [Fact]
    public void Calculate_MixedSuccessAndErrors_ReturnsCorrectMetrics()
    {
        // Arrange
        IMetricsCalculator calculator = new DefaultMetricsCalculator();
        var logs = new List<LogEntry>
        {
            new LogEntry(Guid.NewGuid(), DateTime.UtcNow, "INFO", "test", "log1")
            {
                HttpMethod = "GET",
                HttpPath = "/api/users",
                HttpStatus = 200,
                DurationMs = 50.0
            },
            new LogEntry(Guid.NewGuid(), DateTime.UtcNow, "INFO", "test", "log2")
            {
                HttpMethod = "GET",
                HttpPath = "/api/users",
                HttpStatus = 200,
                DurationMs = 60.0
            },
            new LogEntry(Guid.NewGuid(), DateTime.UtcNow, "ERROR", "test", "log3")
            {
                HttpMethod = "GET",
                HttpPath = "/api/users",
                HttpStatus = 503,
                DurationMs = 5000.0
            },
            new LogEntry(Guid.NewGuid(), DateTime.UtcNow, "INFO", "test", "log4")
            {
                HttpMethod = "POST",
                HttpPath = "/api/users",
                HttpStatus = 201,
                DurationMs = 100.0
            },
            new LogEntry(Guid.NewGuid(), DateTime.UtcNow, "ERROR", "test", "log5")
            {
                HttpMethod = "POST",
                HttpPath = "/api/users",
                HttpStatus = 400,
                DurationMs = 30.0
            }
        };

        // Act
        var result = calculator.Calculate(logs);

        // Assert
        Assert.Equal(2, result.Count);

        var getMetric = result.First(m => m.Endpoint == "GET /api/users");
        Assert.Equal(3, getMetric.TotalRequests);
        Assert.Equal(1, getMetric.ErrorCount);
        Assert.Equal(1703.3333333333333, getMetric.AverageLatencyMs);

        var postMetric = result.First(m => m.Endpoint == "POST /api/users");
        Assert.Equal(2, postMetric.TotalRequests);
        Assert.Equal(1, postMetric.ErrorCount);
        Assert.Equal(65.0, postMetric.AverageLatencyMs);
    }
}
