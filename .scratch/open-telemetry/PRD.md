# PRD: OpenTelemetry Console Observability

Status: ready-for-agent

## Problem Statement

The backend app has no observability. When requests fail or behave unexpectedly, there are no structured logs, no trace correlation, and no metrics to diagnose what happened. The only signal is the HTTP response itself.

## Solution

Add OpenTelemetry console observability using `Microsoft.Extensions.Telemetry` to emit structured JSON logs (enriched with trace IDs and HTTP context), HTTP request metrics, and HTTP-level trace spans for log correlation — all exported to the console for local development.

## User Stories

1. As a developer, I want to see structured JSON logs for every HTTP request in the console, so that I can understand what the app is doing without attaching a debugger.
2. As a developer, I want every log entry to include a trace ID, so that I can correlate all logs belonging to a single request.
3. As a developer, I want log entries to include the HTTP method, path, and status code, so that I can quickly identify which request a log line belongs to.
4. As a developer, I want to see HTTP request metrics (count, duration, status code) in the console, so that I can understand request patterns and performance.
5. As a developer, I want metrics to flush to the console every 5 seconds, so that I get fast feedback during local development.
6. As a developer, I want unhandled exceptions to be logged with their full details and trace ID, so that I can diagnose errors without reproducing them.
7. As a developer, I want the observability setup to follow the existing convention of named DI extension methods, so that the codebase stays consistent.
8. As a developer, I want to be able to swap the console exporter for a real OTLP exporter later without rewriting the configuration, so that I can move to production observability.

## Implementation Decisions

### Modules

1. **Package additions** — `Microsoft.Extensions.Telemetry` and `OpenTelemetry.Exporter.Console` added to `BackendApi.csproj`.
2. **Observability extension** — New `BackendApi/Observability/ObservabilityExtensions.cs` exposing `AddObservability(this IHostApplicationBuilder)`. This is the deep module: encapsulates all OTel configuration behind a single call site.
3. **Program.cs wiring** — Call `builder.AddObservability()` during host building. Update the global exception handler to log exceptions via `ILogger` with trace context.
4. **appsettings.json** — Add `Microsoft.Extensions.Telemetry` logging section to enable enrichment (trace ID, HTTP context).

### Configuration

- **Logs**: JSON console format. Enriched with trace ID, HTTP method, path, status code via `Microsoft.Extensions.Telemetry` log enrichment.
- **Traces**: HTTP request spans only via auto-instrumentation (`AddHttpClientInstrumentation()` / ASP.NET Core auto-spans). No manual spans in use cases.
- **Metrics**: HTTP server metrics only (request count, duration histogram, status code distribution). Console exporter with 5-second push interval.
- **Exception handler**: Uses `ILogger<Program>` to log structured error entries with trace ID, exception type, message, and stack trace.

### Interface shape (ObservabilityExtensions)

```csharp
public static class ObservabilityExtensions
{
    public static void AddObservability(this IHostApplicationBuilder builder);
}
```

Called in `Program.cs` after `WebApplication.CreateBuilder(args)` and before `builder.Build()`.

### appsettings.json additions

The `Microsoft.Extensions.Telemetry` enrichment is configured via the logging configuration section to attach trace ID and HTTP context to log entries.

## Testing Decisions

### What makes a good test

Only test external behavior: that the extension method registers the expected services, and that the exception handler emits log entries with the expected structure. Do not test OTel SDK internals.

### Modules to test

1. **Observability extension** — Verify that calling `AddObservability` registers the expected OTel services (logger provider, tracer provider, meter provider) in the DI container. Similar pattern to existing `AddApplication()` / `AddPostgresStorage()` tests.
2. **Exception handler logging** — Integration test that hits `/error/unhandled` and verifies a log entry was emitted with trace ID and exception details. Similar to existing `ApiIntegrationTests.cs`.

### Prior art

- `BackendApi.Tests/Composition/` — DI wiring tests
- `BackendApi.Tests/ApiIntegrationTests.cs` — HTTP integration tests with `WebApplicationFactory`

## Out of Scope

- Distributed tracing to an external collector (Jaeger, Zipkin, etc.)
- Custom business metrics (e.g., todos_created_total)
- Use-case-level or repository-level trace spans
- Log aggregation or persistence beyond console output
- OpenTelemetry auto-instrumentation agent (CLR profiler)
- Frontend observability

## Further Notes

- The `Microsoft.Extensions.Telemetry` package is Microsoft's opinionated wrapper over the OTel SDK. It provides log enrichment and HTTP metrics out of the box.
- Console exporters are for local development only. Moving to production means swapping exporters (e.g., OTLP) without changing the extension method's interface.
- The existing `/error/unhandled` test endpoint in `Program.cs` is useful for verifying exception logging.
