Status: done

## Parent

- `.scratch/dashboard/PRD.md`

## What to build

Create the `GET /api/dashboard/metrics` endpoint that returns aggregated per-endpoint metrics from the `ILogStore`.

**Endpoint**: `GET /api/dashboard/metrics`

Query parameter:
- `window` — time window: `1h`, `24h` (default), `7d`, `30d`

Returns an array of endpoint metrics. Each entry:

```json
{
  "endpoint": "POST /todos",
  "failureCount": 12,
  "avgDurationMs": 45.3,
  "totalRequests": 150
}
```

- `endpoint` — HTTP method + path (e.g., `POST /todos`)
- `failureCount` — count of requests with status 400-599 in the time window
- `avgDurationMs` — average duration in milliseconds for all requests in the window
- `totalRequests` — total request count in the window

Only endpoints with at least one request in the window are included. The query filters by `timestamp >= NOW() - interval`.

**Tests**: Integration tests following the pattern in `TodoApiIntegrationTests`:
- Seed log entries with various status codes and durations, call with `window=24h`, verify correct aggregation
- Verify failure count includes both 4xx and 5xx
- Verify avg duration calculation is correct
- Call with `window=1h` when all entries are older than 1 hour, verify empty array
- Call with no entries at all, verify empty array

## Acceptance criteria

- [x] `GET /api/dashboard/metrics` endpoint exists and returns JSON array
- [x] `window` query param filters entries by time window (1h, 24h, 7d, 30d)
- [x] Default window is 24h when param is omitted
- [x] Each entry includes endpoint, failureCount, avgDurationMs, totalRequests
- [x] Failure count includes 4xx and 5xx status codes
- [x] Entries grouped by HTTP method + path
- [x] Returns empty array when no entries exist for the window
- [x] Integration tests cover aggregation, time filtering, and empty results
- [x] All existing tests still pass

## Blocked by

- #01-log-persistence-layer
