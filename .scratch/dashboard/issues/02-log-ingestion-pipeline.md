Status: ready-for-agent

## Parent

- `.scratch/dashboard/PRD.md`

## What to build

Build the log ingestion pipeline that captures application activity and writes log entries to the `ILogStore`. This has three parts:

**HTTP Middleware**: An ASP.NET Core middleware that runs at the end of each request pipeline and captures:
- HTTP method, path, status code, duration, and trace ID
- Log level mapping: 200-399 → `info`, 400-499 → `warning`, 500-599 → `error`
- Source: `"middleware"`
- Message: `"{method} {path} returned {status}"`

**Custom ILoggerProvider**: A `PostgresLoggerProvider` that wraps the existing console logger. It forwards all `Log()` calls to the `ILogStore` in addition to the console, capturing non-HTTP logs (startup events, background services, etc.). The source should be the logger category name.

**Exception Handler Integration**: Update the existing global exception handler in `Program.cs` to also write an error-level log entry to the `ILogStore` with:
- Source: `"exception-handler"`
- Exception type and message fields populated
- The existing console logging behavior remains unchanged

**Tests**: Integration tests following the pattern in `TodoApiIntegrationTests`:
- Make a request that returns 200, verify an `info` log entry is written with correct method, path, status, and duration
- Make a request that returns 400, verify a `warning` log entry is written
- Hit `/error/unhandled`, verify an `error` log entry is written with exception type and message
- Verify trace ID is captured on log entries

## Acceptance criteria

- [ ] Middleware captures HTTP request data and writes log entries to `ILogStore`
- [ ] Middleware maps status codes to correct log levels (200-399=info, 400-499=warning, 500-599=error)
- [ ] Custom `ILoggerProvider` forwards logs to `ILogStore` alongside console output
- [ ] Exception handler writes error log entries with exception type and message
- [ ] Existing console logging behavior is unchanged
- [ ] Integration tests verify log entry creation for success, client error, and server error scenarios
- [ ] All existing tests still pass

## Blocked by

- #01-log-persistence-layer
