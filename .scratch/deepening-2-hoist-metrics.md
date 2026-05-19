# Deepening Opportunity 2: Hoist Metrics Logic to Use Case

**Severity:** 🔴 CRITICAL  
**Status:** Not started  
**Complexity:** Medium

## Problem Statement

Metric aggregation logic is embedded directly in the HTTP endpoint handler in `Program.cs`. This violates separation of concerns and makes the logic untestable in isolation.

### Current State

**File:** `BackendApi/Program.cs` (lines 187-194)

```csharp
app.MapGet("/metrics", async (HttpContext context, ILogStore logStore) =>
{
    var window = context.Request.Query["window"].ToString() ?? "1h";
    var logs = await logStore.QueryAsync(new LogFilter
    {
        Since = window switch
        {
            "1h" => DateTime.UtcNow.AddHours(-1),
            "7d" => DateTime.UtcNow.AddDays(-7),
            "30d" => DateTime.UtcNow.AddDays(-30),
            _ => DateTime.UtcNow.AddHours(-1),
        },
    }, default);

    // ← BUSINESS LOGIC EMBEDDED HERE
    var metrics = entries
        .GroupBy(e => $"{e.HttpMethod} {e.HttpPath}")
        .Select(g => new EndpointMetric(
            g.Key,
            g.Count(e => e.HttpStatus >= 400),
            g.Average(e => e.DurationMs ?? 0),
            g.Count()))
        .ToList();

    var response = new MetricsResponse(metrics);
    return Results.Ok(response);
});
```

**Why This Is Shallow:**
- **Interface:** endpoint receives query param, returns metrics
- **Implementation:** metric aggregation (grouping, averaging, error counting) lives inside endpoint handler
- **No abstraction:** business logic is tightly coupled to HTTP context

### Testing Impact

**Current test burden:**
- Cannot test metric calculation independently from HTTP layer
- Must set up `HttpContext` mock or test through full HTTP request
- Adding new metric types requires endpoint changes
- Reusing metric logic for different queries (e.g., dashboard vs. API response) requires duplication

**Example: untestable as-is**
```csharp
[Fact]
public async Task CalculatesAverageLatency()
{
    // To test metric calculation, must mock:
    // 1. HttpContext
    // 2. ILogStore
    // 3. Make HTTP request to /metrics endpoint
    // Result: testing is brittle and slow
}
```

### Scope

**Files affected:**
- `BackendApi/Program.cs` — remove calculation logic
- `BackendApi.Application/UseCases/CalculateMetricsUseCase.cs` — new
- `BackendApi.Application/Services/MetricsCalculator.cs` — new
- `BackendApi.Tests/*` — add unit tests for use case

## Solution

Extract metrics calculation to a **use case** in the Application layer:

### 1. Create Domain Value Object

**File:** `BackendApi.Domain/EndpointMetric.cs`

```csharp
namespace BackendApi.Domain;

/// <summary>
/// Aggregated metrics for a single endpoint over a time window.
/// </summary>
public record EndpointMetric
{
    public required string Endpoint { get; init; }
    public required int ErrorCount { get; init; }
    public required double AverageLatencyMs { get; init; }
    public required int TotalRequests { get; init; }
    
    public double ErrorRate => TotalRequests > 0 
        ? (double)ErrorCount / TotalRequests 
        : 0;
    
    public bool IsHealthy => ErrorRate < 0.05; // < 5% error rate
}
```

### 2. Create Ports

**File:** `BackendApi.Application/Ports/IMetricsCalculator.cs`

```csharp
namespace BackendApi.Application.Ports;

public interface IMetricsCalculator
{
    /// <summary>
    /// Aggregates log entries into endpoint metrics.
    /// </summary>
    IReadOnlyList<EndpointMetric> Calculate(IReadOnlyList<LogEntry> logs);
}
```

### 3. Create Use Case

**File:** `BackendApi.Application/UseCases/CalculateMetricsUseCase.cs`

```csharp
namespace BackendApi.Application.UseCases;

using BackendApi.Application.Ports;
using BackendApi.Domain;

public class CalculateMetricsUseCase
{
    private readonly ILogStore _logStore;
    private readonly IMetricsCalculator _calculator;

    public CalculateMetricsUseCase(ILogStore logStore, IMetricsCalculator calculator)
    {
        _logStore = logStore;
        _calculator = calculator;
    }

    /// <summary>
    /// Retrieves logs for the given time window and calculates metrics.
    /// </summary>
    public async Task<IReadOnlyList<EndpointMetric>> Execute(TimeSpan window, CancellationToken ct = default)
    {
        var since = DateTime.UtcNow - window;
        var logs = await _logStore.QueryAsync(new LogFilter { Since = since }, ct);
        return _calculator.Calculate(logs);
    }
}
```

### 4. Create Calculator Implementation

**File:** `BackendApi.Application/Services/DefaultMetricsCalculator.cs`

```csharp
namespace BackendApi.Application.Services;

using BackendApi.Application.Ports;
using BackendApi.Domain;

public class DefaultMetricsCalculator : IMetricsCalculator
{
    public IReadOnlyList<EndpointMetric> Calculate(IReadOnlyList<LogEntry> logs)
    {
        if (logs.Count == 0)
            return new List<EndpointMetric>();

        var metrics = logs
            .GroupBy(e => $"{e.HttpMethod} {e.HttpPath}")
            .Select(group => new EndpointMetric
            {
                Endpoint = group.Key,
                ErrorCount = group.Count(e => e.HttpStatus >= 400),
                AverageLatencyMs = group.Average(e => e.DurationMs ?? 0),
                TotalRequests = group.Count(),
            })
            .ToList();

        return metrics;
    }
}
```

### 5. Create Time Window Abstraction

**File:** `BackendApi.Application/Services/MetricsTimeWindow.cs`

```csharp
namespace BackendApi.Application.Services;

/// <summary>
/// Predefined time windows for metrics queries.
/// Prevents magic strings like "1h", "7d" from scattering through the codebase.
/// </summary>
public static class MetricsTimeWindow
{
    public static TimeSpan Parse(string? window) => window switch
    {
        "1h" => TimeSpan.FromHours(1),
        "7d" => TimeSpan.FromDays(7),
        "30d" => TimeSpan.FromDays(30),
        _ => TimeSpan.FromHours(1), // Default to 1 hour
    };

    public const string OneHour = "1h";
    public const string SevenDays = "7d";
    public const string ThirtyDays = "30d";
}
```

### 6. Update Endpoint

**File:** `BackendApi/Program.cs`

```csharp
// After building services
var app = builder.Build();

// Register use case (in dependency injection setup)
services.AddScoped<CalculateMetricsUseCase>();
services.AddSingleton<IMetricsCalculator, DefaultMetricsCalculator>();

// Then in endpoint handler:
app.MapGet("/metrics", async (HttpContext context, CalculateMetricsUseCase calculateMetrics) =>
{
    var window = context.Request.Query["window"].ToString();
    var timeWindow = MetricsTimeWindow.Parse(window);
    
    var metrics = await calculateMetrics.Execute(timeWindow, context.RequestAborted);
    var response = new MetricsResponse(metrics);
    
    return Results.Ok(response);
});
```

## Benefits

### Locality
- **Metric calculation concentrated:** All aggregation logic lives in one place (`DefaultMetricsCalculator`)
- **Time window logic centralized:** `MetricsTimeWindow` defines all window values
- **Change efficiency:** To add error rate calculation or new metrics, update calculator; to add window, update enum

### Leverage
- **Reusable calculation:** Dashboard, alerts, exports can all reuse `CalculateMetricsUseCase`
- **Pluggable calculator:** `IMetricsCalculator` allows different implementations (e.g., caching, real-time streaming)
- **Testable in isolation:** Test metric calculation without mocking HTTP context

### Testing

**Before:** Metrics calculation tightly coupled to HTTP endpoint
```csharp
[Fact]
public async Task CalculatesAverageLatency()
{
    // Must set up full HTTP context and endpoint
    // Brittle, slow
}
```

**After:** Metrics calculation independently testable
```csharp
[Fact]
public void CalculatesAverageLatency()
{
    var logs = new[]
    {
        new LogEntry(Guid.NewGuid(), DateTime.UtcNow, "info", "api", "GET /todos", 
            "GET", "/todos", 200, 50),
        new LogEntry(Guid.NewGuid(), DateTime.UtcNow, "info", "api", "GET /todos", 
            "GET", "/todos", 200, 150),
    };

    var calculator = new DefaultMetricsCalculator();
    var metrics = calculator.Calculate(logs);

    Assert.Single(metrics);
    Assert.Equal(100, metrics[0].AverageLatencyMs);
    Assert.Equal(0, metrics[0].ErrorCount);
}

[Fact]
public async Task RetrievesAndAggregatesLogs()
{
    var mockStore = new Mock<ILogStore>();
    mockStore.Setup(s => s.QueryAsync(It.IsAny<LogFilter>(), default))
        .ReturnsAsync(new List<LogEntry> { /* test data */ });

    var calculator = new Mock<IMetricsCalculator>();
    calculator.Setup(c => c.Calculate(It.IsAny<IReadOnlyList<LogEntry>>()))
        .Returns(new List<EndpointMetric> { /* expected result */ });

    var useCase = new CalculateMetricsUseCase(mockStore.Object, calculator.Object);
    var result = await useCase.Execute(TimeSpan.FromHours(1));

    mockStore.Verify(s => s.QueryAsync(
        It.Is<LogFilter>(f => f.Since.HasValue),
        default));
    Assert.NotEmpty(result);
}
```

## Implementation Checklist

- [ ] Create `BackendApi.Domain/EndpointMetric.cs`
- [ ] Create `BackendApi.Application/Ports/IMetricsCalculator.cs`
- [ ] Create `BackendApi.Application/UseCases/CalculateMetricsUseCase.cs`
- [ ] Create `BackendApi.Application/Services/DefaultMetricsCalculator.cs`
- [ ] Create `BackendApi.Application/Services/MetricsTimeWindow.cs`
- [ ] Update `BackendApi/Program.cs` endpoint to use use case
- [ ] Register use case and calculator in dependency injection
- [ ] Write unit tests for `DefaultMetricsCalculator`
- [ ] Write unit tests for `CalculateMetricsUseCase`
- [ ] Verify `GET /metrics?window=1h` still works
- [ ] Update API contract docs if window format changes

## Related Deepening Opportunities

- **Deepening 3:** Remove Identity Pass-Through Use Cases (similar pattern: eliminate thin wrappers)
