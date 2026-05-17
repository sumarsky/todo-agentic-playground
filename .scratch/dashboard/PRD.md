Status: ready-for-agent

## Problem Statement

The application emits OpenTelemetry logs, traces, and metrics to the console, but they are never consumed or visible to anyone running the app. Metrics are reported every 5 seconds but discarded. When API requests fail or run slowly, there is no way to inspect what happened without reading raw console output. Developers and operators have no visibility into endpoint health, failure rates, or performance.

## Solution

Add an Observability Dashboard and a Log Viewer page to the React frontend, backed by new API endpoints that persist log entries to PostgreSQL. The home page gets two navigation buttons ("Dashboard" and "Logs") as entry points. The Dashboard shows per-endpoint failure counts and average durations over a configurable time window. The Log Viewer shows all persisted log entries with filtering by message text and log level (info, warning, error), ordered by timestamp.

## User Stories

1. As a developer, I want to click a "Dashboard" button on the home page so that I can navigate to the observability dashboard.
2. As a developer, I want to click a "Logs" button on the home page so that I can navigate to the log viewer.
3. As a developer, I want the dashboard to show the total number of failures (4xx + 5xx) per API endpoint so that I can identify which endpoints are erroring most.
4. As a developer, I want the dashboard to show the average response duration per API endpoint so that I can spot slow endpoints.
5. As a developer, I want to select a time window (1h, 24h, 7d, 30d) on the dashboard so that I can view metrics over different periods.
6. As a developer, I want each endpoint's metrics displayed as a card so that I can scan the dashboard quickly.
7. As a developer, I want to see all persisted log entries on the Logs page so that I can review what the application has logged.
8. As a developer, I want to filter logs by log level (info, warning, error) using a dropdown so that I can focus on specific severity.
9. As a developer, I want to filter logs by searching the log message text so that I can find specific events.
10. As a developer, I want logs to be ordered by timestamp (newest first) so that I see the most recent activity first.
11. As a developer, I want both filters (message search and level) to be combinable so that I can narrow down precisely.
12. As a developer, I want the dashboard to default to a 24-hour time window so that I immediately see recent activity without configuring anything.
13. As a developer, I want the logs page to load all matching entries at once so that I don't need pagination for typical usage.
14. As a developer, I want the dashboard to handle the case where no data exists for the selected window so that I don't see broken UI.
15. As a developer, I want the home page buttons to be visible and clearly labeled so that I can discover the observability features.
16. As a developer, I want the dashboard to group metrics by HTTP method + path (e.g., `POST /todos`) so that I can distinguish between endpoints on the same path.
17. As a developer, I want logs to include HTTP method, path, status code, and duration when they originate from a request so that I can correlate logs with specific API calls.
18. As a developer, I want error logs to include the exception type and message so that I can diagnose failures without checking the console.
19. As a developer, I want the dashboard to update when I change the time window without a full page reload so that the experience is smooth.
20. As a developer, I want the logs page to show the source of each log entry (middleware, exception handler, or custom logger) so that I know where the log originated.

## Implementation Decisions

### Routing

- Add `react-router-dom` to the frontend.
- Restructure `App.jsx` to use `<BrowserRouter>` with routes: `/` (home), `/dashboard`, `/logs`.
- The home page (`/`) renders the existing `<TodoList />` component plus two navigation buttons: "Dashboard" and "Logs".

### Modules to Build

**Backend — Log Persistence (deep module)**

- A `logs` table in PostgreSQL with the following schema:

```sql
CREATE TABLE logs (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    timestamp timestamptz NOT NULL DEFAULT now(),
    level varchar(10) NOT NULL,
    source varchar(100) NOT NULL,
    message text NOT NULL,
    http_method varchar(10),
    http_path varchar(500),
    http_status int,
    duration_ms double precision,
    trace_id varchar(50),
    exception_type varchar(200),
    exception_message text
);
```

- A new `ILogStore` port in the application layer with methods:
  - `WriteAsync(LogEntry entry)` — writes a single log entry
  - `QueryAsync(LogFilter filter)` — returns filtered log entries ordered by timestamp descending
- A `PostgresLogStore` implementation in the storage layer using Dapper + Npgsql.
- A `LogEntry` record and `LogFilter` record in the application layer.

**Backend — Log Ingestion**

- **Middleware**: An ASP.NET Core middleware that captures each HTTP request's method, path, status code, duration, and trace ID, then writes a log entry at the end of the pipeline. Status codes 400-599 produce `warning` or `error` level; 200-399 produce `info`.
- **Custom ILoggerProvider**: A `PostgresLoggerProvider` that wraps the existing console logger, forwarding all `Log()` calls to the `ILogStore` in addition to the console. This captures non-HTTP logs (e.g., startup events, background service logs).
- **Exception Handler Integration**: The existing global exception handler is updated to also write error-level log entries to the `ILogStore` with exception type and message.

**Backend — API Endpoints**

- `GET /api/logs?level=info|warning|error&message=<text>` — returns filtered log entries as JSON. All query params are optional. Returns array of log entries ordered by timestamp descending.
- `GET /api/dashboard/metrics?window=1h|24h|7d|30d` — returns aggregated metrics per endpoint (method + path). Response shape:

```json
[
  {
    "endpoint": "POST /todos",
    "failureCount": 12,
    "avgDurationMs": 45.3,
    "totalRequests": 150
  }
]
```

- Both endpoints are read-only and require no authentication (same as existing API).

**Frontend — Dashboard Page**

- New page component at `/dashboard`.
- Fetches metrics from `GET /api/dashboard/metrics?window=<selected>`.
- Renders one card per endpoint showing: endpoint name (method + path), failure count, average duration, total requests.
- Time window selector: dropdown with options `1h`, `24h` (default), `7d`, `30d`.
- Cards are displayed in a responsive grid layout.
- Empty state: when no data exists for the selected window, show a "No data for this time period" message.

**Frontend — Logs Page**

- New page component at `/logs`.
- Fetches logs from `GET /api/logs` with optional `level` and `message` query params.
- Filter controls: dropdown for level (All, Info, Warning, Error), text input for message search.
- Logs displayed in a table or list, ordered by timestamp descending (newest first).
- Each row shows: timestamp, level (color-coded), source, message, and HTTP details when available.
- All matching logs loaded at once (no pagination).

**Frontend — Home Page Changes**

- Add two buttons below the existing `<TodoList />` on the home page: "Dashboard" and "Logs".
- Buttons use `react-router-dom`'s `Link` component for client-side navigation.

### Schema Changes

- New `logs` table in PostgreSQL (see schema above).
- Migration file: `002-create-logs-table.sql` added to `BackendApi/Migrations/`.

### API Contracts

New endpoints added to the API. Existing endpoints are unchanged.

### Failure Definition

A "failure" is any HTTP response with status code 400-599 (both client errors and server errors). This is consistent across the dashboard and log ingestion.

### Log Level Mapping

- HTTP 200-399 → `info`
- HTTP 400-499 → `warning`
- HTTP 500-599 → `error`
- Custom logger calls use the level passed to the `ILogger` interface.

### Time Window Mapping

- `1h` → last 1 hour from now
- `24h` → last 24 hours from now
- `7d` → last 7 days from now
- `30d` → last 30 days from now

## Testing Decisions

**What makes a good test**: Only test external behavior, not implementation details. Tests should verify that given certain inputs, the system produces the correct outputs — regardless of how the internals are structured.

**Modules to test**:

1. **`PostgresLogStore`** — Integration tests (similar to `PostgresTodoRepositoryTests`):
   - Write a log entry, then query it back with various filters.
   - Verify ordering by timestamp.
   - Verify filtering by level and message text.
   - Verify empty results when no entries match.

2. **Log ingestion middleware** — Integration tests (similar to `TodoApiIntegrationTests`):
   - Make a request that returns 200, verify an `info` log entry is written.
   - Make a request that returns 400, verify a `warning` log entry is written.
   - Make a request that throws, verify an `error` log entry is written with exception details.
   - Verify duration and trace ID are captured.

3. **`GET /api/logs` endpoint** — Integration tests:
   - Seed log entries, call the endpoint with various query params, verify correct filtering and ordering.
   - Verify empty array when no entries match.

4. **`GET /api/dashboard/metrics` endpoint** — Integration tests:
   - Seed log entries with various status codes and durations, call the endpoint with different time windows, verify correct aggregation.
   - Verify failure count includes both 4xx and 5xx.
   - Verify avg duration calculation.
   - Verify empty array when no entries exist for the window.

5. **Dashboard page component** — Component tests (similar to Vitest tests in `frontend/src/components/`):
   - Renders cards when data is present.
   - Renders empty state when no data.
   - Time window selector triggers re-fetch.

6. **Logs page component** — Component tests:
   - Renders log entries when data is present.
   - Filter controls update query params and trigger re-fetch.
   - Level dropdown filters correctly.
   - Message search filters correctly.

7. **Home page navigation** — Component tests:
   - Dashboard and Logs buttons render and navigate to correct routes.

**Prior art**:
- Backend integration tests: `BackendApi.Tests/TodoApiIntegrationTests.cs`
- Backend storage tests: `BackendApi.Tests/Storage/Postgres/PostgresTodoRepositoryTests.cs`
- Frontend component tests: `frontend/src/components/` tested with Vitest + Testing Library
- Observability wiring tests: `BackendApi.Tests/Observability/ObservabilityWiringTests.cs`

## Out of Scope

- Real-time log streaming (Server-Sent Events, WebSockets). Logs are fetched on demand.
- Log retention policies or automatic log cleanup.
- Authentication or authorization for dashboard/logs pages.
- Exporting logs (CSV, JSON download).
- Custom alerting or notifications based on metrics thresholds.
- Distributed tracing visualization (trace spans, waterfall views).
- Log aggregation from multiple application instances.
- Modifying existing todo CRUD endpoints or domain logic.
- Frontend error logging (only backend logs are persisted).
- Pagination on the logs page.

## Further Notes

- The `ILogStore` port is intentionally simple — `WriteAsync` and `QueryAsync` — so it can be replaced with a different storage backend if needed (e.g., Elasticsearch, file-based) without changing the application layer.
- The middleware writes log entries synchronously at the end of the pipeline. If performance becomes a concern, this can be changed to async batched writes behind the same interface.
- The existing OpenTelemetry console exporters remain active — the new persistence layer is additive, not a replacement.
- The `/error/unhandled` test endpoint will produce error log entries when hit, which is useful for verifying the dashboard.
- The `logs` table will grow over time. A future ADR may address retention/cleanup strategy, but it is out of scope for this feature.
