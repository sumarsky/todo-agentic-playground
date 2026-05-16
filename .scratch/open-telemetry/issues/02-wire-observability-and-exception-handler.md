# 02 — Wire ObservabilityExtensions into Program.cs and update exception handler

Status: done

## Description

Call `builder.AddObservability()` in `Program.cs` and update the global exception handler to log exceptions with trace context.

## Acceptance Criteria

- `builder.AddObservability()` called in `Program.cs` during host building (after `CreateBuilder`, before `Build`)
- Global exception handler updated to:
  - Accept/resolve `ILogger<Program>` (or equivalent)
  - Log the exception with structured properties: trace ID, exception type, message, stack trace
  - Preserve existing behavior: returns JSON `{ "error": "..." }` with 500 status
- `appsettings.json` updated with any required `Microsoft.Extensions.Telemetry` logging enrichment configuration
- Running the app and hitting any endpoint produces JSON log output in the console with trace IDs
- Hitting `/error/unhandled` logs the full exception details with trace ID

## Dependencies

- Depends on **01 — Add OTel packages and create ObservabilityExtensions**

## Notes

- See `.scratch/open-telemetry/PRD.md` for full context.
- The `/error/unhandled` endpoint already exists in `Program.cs` for testing.
- The existing exception handler uses `IExceptionHandlerPathFeature` to get the error — keep this, just add logging.
