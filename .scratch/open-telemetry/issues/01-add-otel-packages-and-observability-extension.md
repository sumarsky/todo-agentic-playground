# 01 — Add OTel packages and create ObservabilityExtensions

Status: ready-for-agent

## Description

Add the OpenTelemetry NuGet packages to `BackendApi.csproj` and create the `ObservabilityExtensions` class in `BackendApi/Observability/` that encapsulates all observability configuration.

## Acceptance Criteria

- `Microsoft.Extensions.Telemetry` and `OpenTelemetry.Exporter.Console` packages added to `BackendApi.csproj`
- `BackendApi/Observability/ObservabilityExtensions.cs` created with `AddObservability(this IHostApplicationBuilder)` extension method
- Method configures:
  - **Logs**: JSON console exporter, enriched with trace ID, HTTP method, path, and status code
  - **Traces**: HTTP request spans via auto-instrumentation (no manual spans)
  - **Metrics**: HTTP server metrics (count, duration, status code) with console exporter at 5-second interval
- Method is a single public static extension method following the existing DI convention (`AddApplication()`, `AddPostgresStorage()`)

## Dependencies

- None — this is the first slice.

## Notes

- See `.scratch/open-telemetry/PRD.md` for full context.
- The extension should be self-contained so it can be tested in isolation.
- Console exporters are for local dev; the interface should allow swapping exporters later.
