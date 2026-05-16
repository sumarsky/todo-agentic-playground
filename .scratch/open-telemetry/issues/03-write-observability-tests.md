# 03 — Write tests for Observability wiring and exception handler logging

Status: ready-for-agent

## Description

Write tests to verify the observability extension registers the expected services and that the exception handler emits structured log entries with trace context.

## Acceptance Criteria

- **Observability wiring test** (in `BackendApi.Tests/Composition/` or new `BackendApi.Tests/Observability/`):
  - Builds a host with `AddObservability()` and verifies that OTel services are registered in DI (logger provider, tracer provider, meter provider)
  - Follows the same pattern as existing `LayerRegistrationTests.cs`
- **Exception handler logging test** (in `BackendApi.Tests/TodoApiIntegrationTests.cs` or new file):
  - Uses `WebApplicationFactory<Program>` to send a request to `/error/unhandled`
  - Verifies the response is a 500 with JSON error body (existing behavior preserved)
  - Verifies a log entry was emitted containing the exception details and trace ID
  - Follows the same pattern as existing `ApiIntegrationTests.cs`
- All existing tests continue to pass (`dotnet test`)

## Dependencies

- Depends on **02 — Wire ObservabilityExtensions into Program.cs and update exception handler**

## Notes

- See `.scratch/open-telemetry/PRD.md` for full context.
- Prior art: `BackendApi.Tests/Composition/LayerRegistrationTests.cs` for DI tests, `BackendApi.Tests/ApiIntegrationTests.cs` for HTTP integration tests.
- Do not test OTel SDK internals — only verify services are registered and logs are emitted.
