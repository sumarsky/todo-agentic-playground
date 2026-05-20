# Issue: Create Application Model & Calculator Port

## Parent

Deepening 2: Hoist Metrics Logic to Use Case

## What to build

Create the foundational Application layer types for metrics calculation:

1. **`EndpointMetric` (Application model)**: An immutable value object representing aggregated metrics for a single endpoint. Fields: `Endpoint` (string), `ErrorCount` (int), `AverageLatencyMs` (double), `TotalRequests` (int). Computed properties: `ErrorRate` (double, calculated as ErrorCount / TotalRequests), `IsHealthy` (boolean, true if ErrorRate < 0.05).

2. **`IMetricsCalculator` (port)**: An interface defining the aggregation contract. Single method: `Calculate(IReadOnlyList<LogEntry> logs) → IReadOnlyList<EndpointMetric>`. No state, no dependencies.

These types are the contract that the use case and calculator implementation will work against.

## Acceptance criteria

- [x] `BackendApi.Application/Models/EndpointMetric.cs` exists as an immutable record with all fields and computed properties
- [x] `ErrorRate` calculation handles zero total requests (returns 0)
- [x] `IsHealthy` threshold is hardcoded to 5% error rate
- [x] `BackendApi.Application/Ports/IMetricsCalculator.cs` exists with the `Calculate` method signature
- [x] Both types compile without errors

## Blocked by

None - can start immediately.
