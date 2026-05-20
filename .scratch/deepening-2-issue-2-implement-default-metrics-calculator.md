# Issue: Implement DefaultMetricsCalculator

## Parent

Deepening 2: Hoist Metrics Logic to Use Case

## What to build

Implement `IMetricsCalculator` with the core metrics aggregation logic currently embedded in the endpoint handler. The implementation must:

1. **Filter complete HTTP logs**: Only process logs where `HttpMethod`, `HttpPath`, `HttpStatus`, and `DurationMs` are all present. Skip non-HTTP logs and logs with missing instrumentation data.

2. **Group by endpoint**: Group filtered logs by the key `"{HttpMethod} {HttpPath}"` (e.g., "GET /todos", "POST /api/metrics").

3. **Calculate per-endpoint metrics**:
   - `ErrorCount`: Count of logs where `HttpStatus >= 400`
   - `TotalRequests`: Count of all logs in the group
   - `AverageLatencyMs`: Average of `DurationMs` across the group
   - `Endpoint`: The grouped key

4. **Handle empty input**: If the input list is empty, return an empty list. Do not throw.

The calculator is a pure function — no state, no side effects.

## Acceptance criteria

- [x] `BackendApi.Application/Services/DefaultMetricsCalculator.cs` implements `IMetricsCalculator`
- [x] Empty input list returns empty output list
- [x] Logs without all four HTTP fields (`HttpMethod`, `HttpPath`, `HttpStatus`, `DurationMs`) are filtered out
- [x] Logs are grouped correctly by `"{HttpMethod} {HttpPath}"` 
- [x] Error count only includes status codes >= 400
- [x] Average latency is calculated only from logs with non-null `DurationMs`
- [x] Multiple requests to the same endpoint produce one aggregated metric
- [x] Unit tests in `BackendApi.Tests/Application/DefaultMetricsCalculatorTests.cs` cover: empty logs, filtering, grouping, error counting, latency averaging, mixed error/success scenarios

## Blocked by

deepening-2-issue-1-application-model-and-calculator-port.md
