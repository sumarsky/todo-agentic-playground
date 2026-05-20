# Issue: Update Endpoint & Wiring

## Parent

Deepening 2: Hoist Metrics Logic to Use Case

## What to build

Refactor the `GET /metrics` endpoint to use the new use case architecture, and wire up the Application layer in the composition root.

1. **Endpoint refactor** in `Program.cs`:
   - Parse time window query parameter inline with a switch expression (no helper class):
     - "1h" → `TimeSpan.FromHours(1)`
     - "7d" → `TimeSpan.FromDays(7)`
     - "30d" → `TimeSpan.FromDays(30)`
     - missing/unrecognized → `TimeSpan.FromHours(1)` (default)
   - Inject `CalculateMetricsUseCase` as a parameter
   - Call `await calculateMetrics.Execute(timeWindow, context.RequestAborted)`
   - Map the Application model `EndpointMetric` to the contract `EndpointMetric` (tuple) by extracting: `Endpoint`, `ErrorCount` (as `FailureCount`), `AverageLatencyMs` (as `AvgDurationMs`), `TotalRequests`
   - Return `Results.Ok(new MetricsResponse(mappedMetrics))`

2. **DI wiring** in composition root (top of `Program.cs`):
   - Call `builder.Services.AddApplication()` to register use case and calculator

## Acceptance criteria

- [x] `GET /metrics` endpoint compiles and runs
- [x] Endpoint injects `CalculateMetricsUseCase` as a dependency
- [x] Query parameter parsing works: "1h" → 1 hour, "7d" → 7 days, "30d" → 30 days, default → 1 hour
- [x] Endpoint calls `calculateMetrics.Execute(timeWindow, context.RequestAborted)`
- [x] Application model is mapped to contract before response (Application `ErrorCount` → Contract `FailureCount`, `AverageLatencyMs` → `AvgDurationMs`)
- [x] `builder.Services.AddApplication()` is called in `Program.cs`
- [x] `GET /metrics?window=1h` returns 200 with metrics array
- [x] `GET /metrics?window=7d` returns 200 with metrics array
- [x] `GET /metrics?window=30d` returns 200 with metrics array
- [x] `GET /metrics` (no window) defaults to 1h, returns 200
- [x] `GET /metrics?window=invalid` defaults to 1h, returns 200
- [x] All existing tests pass (no regression)
- [x] Endpoint integration tests verify request/response flow

## Blocked by

deepening-2-issue-3-create-use-case-and-di.md
