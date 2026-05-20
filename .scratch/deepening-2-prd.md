# PRD: Extract Metrics Calculation to Application Layer

## Problem Statement

Metric aggregation logic is embedded directly in the HTTP endpoint handler (`GET /metrics`). This violates separation of concerns — the Application layer should own business logic, not the HTTP layer. As a result:

- Metric calculation cannot be tested in isolation without mocking HTTP context
- Reusing metric logic for different entry points (dashboards, alerts, exports) requires duplication
- Adding new metric types or calculation strategies requires endpoint changes

## Solution

Extract metrics calculation into a dedicated Application layer use case (`CalculateMetricsUseCase`) that orchestrates `ILogStore` and `IMetricsCalculator`. The endpoint becomes a thin HTTP adapter that:

1. Parses the query parameter into a `TimeSpan`
2. Calls the use case
3. Maps Application model to HTTP contract
4. Returns the response

This creates a clear separation: Application layer owns calculation logic; HTTP layer owns request/response mapping.

## User Stories

1. As a metrics API consumer, I want to retrieve endpoint metrics for a given time window, so that I can monitor system health
2. As a metrics dashboard, I want to reuse the metrics calculation logic without calling the HTTP endpoint, so that I can avoid unnecessary network latency
3. As a developer, I want to test metric calculation independently from HTTP mocking, so that tests are fast and focused
4. As a developer, I want to add new metric types (e.g., percentiles, throughput), so that I don't have to touch the endpoint handler
5. As a developer, I want to swap metric calculators (e.g., real-time streaming vs batch aggregation), so that I can evolve the implementation without breaking the API
6. As an API operator, I want only complete HTTP logs to contribute to metrics, so that missing data doesn't skew averages
7. As a monitoring system, I want to know if an endpoint is healthy, so that I can alert on degradation

## Implementation Decisions

### Layers and Module Boundaries

**Application Layer owns metric calculation:**
- `CalculateMetricsUseCase`: Orchestrates `ILogStore` query + `IMetricsCalculator` aggregation. Receives `TimeSpan` (not HTTP strings). Returns Application model.
- `IMetricsCalculator`: Port defining the aggregation contract. Receives raw `LogEntry` list, returns aggregated metrics.
- `DefaultMetricsCalculator`: Implementation that groups logs by endpoint, calculates error count, average latency, total requests, error rate, and health status.
- `EndpointMetric` (Application model): Rich value object with computed properties (`ErrorRate`, `IsHealthy`). Lives in Application layer, not Domain.

**HTTP Layer stays thin:**
- Endpoint parses query string into `TimeSpan` inline (no helper class).
- Calls use case.
- Maps Application `EndpointMetric` to contract `EndpointMetric`.
- Returns HTTP response.

**Dependency Injection:**
- Create `AddApplication()` extension in `BackendApi.Application/Extensions/` to register use case and calculator.
- Call from `Program.cs` composition root.
- `DefaultMetricsCalculator` registered as `Singleton` (stateless).
- `CalculateMetricsUseCase` registered as `Scoped`.

### Data Filtering

Only logs with **complete HTTP information** contribute to metrics:
- `HttpMethod` must be non-null
- `HttpPath` must be non-null
- `HttpStatus` must have value
- `DurationMs` must have value

Rationale: Non-HTTP logs (domain events, errors without duration) are excluded. Null duration means incomplete instrumentation — skip rather than treat as 0ms.

### Model Separation

Two `EndpointMetric` types coexist:
- **Contract** (`BackendApi/Contracts/EndpointMetric`): Simple tuple for HTTP response. Unchanged.
- **Application** (`BackendApi.Application/Models/EndpointMetric`): Rich model with `ErrorRate` and `IsHealthy` computed properties. Used internally by use case and calculator.

Endpoint maps Application model to contract before returning.

### Health Threshold

`IsHealthy` threshold hardcoded to 5% error rate (`ErrorRate < 0.05`). Rationale: reasonable default; easy to extract to config if future needs require it. Not a config lever now.

### Testing Strategy

- **Unit test `DefaultMetricsCalculator` in isolation**: Pure function, no dependencies. Test aggregation math, filtering logic, edge cases (empty logs, missing fields, error counting).
- **Unit test `CalculateMetricsUseCase` with real calculator**: Mock only `ILogStore`. Test full flow: store query → calculation → result mapping. Validates orchestration and error handling.
- Tests should focus on external behavior (inputs → outputs), not implementation details (how grouping is done).

### Time Window Parsing

Query string values ("1h", "7d", "30d") parsed in endpoint handler via switch expression. No helper class. Rationale: HTTP concern belongs in HTTP layer, not Application.

Default: "1h" if missing or unrecognized.

## Out of Scope

- Caching metrics results (considered future optimization)
- Custom time window formats (e.g., "2h 30m")
- Streaming or real-time metrics (calculator always works on completed log batch)
- Percentile-based latency metrics (error rate + average sufficient for MVP)
- Exporting metrics to external systems (e.g., Prometheus)
- Alerting on health threshold breaches (consumer's responsibility)

## Further Notes

This deepening improves testability and reusability. The pattern aligns with existing ADR-0001 (split backend into layered projects) and ADR-0002 (extract frontend context hooks) — clear layer boundaries, testable in isolation, loosely coupled via ports.

Future deepenings may apply the same pattern to other embedded logic (e.g., logging enrichment, error categorization).
