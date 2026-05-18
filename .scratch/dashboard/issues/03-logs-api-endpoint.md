Status: done

## Parent

- `.scratch/dashboard/PRD.md`

## What to build

Create the `GET /api/logs` endpoint that returns filtered log entries from the `ILogStore`.

**Endpoint**: `GET /api/logs`

Query parameters (all optional):
- `level` — filter by log level: `info`, `warning`, `error`
- `message` — filter by message text (partial match, case-insensitive)

Returns an array of log entries ordered by timestamp descending (newest first). Each entry includes: id, timestamp, level, source, message, and HTTP fields (method, path, status, duration_ms) when available, plus trace_id and exception fields when present.

**Tests**: Integration tests following the pattern in `TodoApiIntegrationTests`:
- Seed log entries via `ILogStore`, call the endpoint with no params, verify all entries returned in correct order
- Call with `level=error`, verify only error entries returned
- Call with `message=todos`, verify only matching entries returned
- Call with both `level` and `message`, verify combined filtering works
- Call when no entries exist, verify empty array returned

## Acceptance criteria

- [ ] `GET /api/logs` endpoint exists and returns JSON array
- [ ] Entries ordered by timestamp descending
- [ ] `level` query param filters by log level
- [ ] `message` query param filters by message text (partial match)
- [ ] Both filters are combinable
- [ ] Returns empty array when no entries match
- [ ] Integration tests cover all filtering scenarios
- [ ] All existing tests still pass

## Blocked by

- #01-log-persistence-layer
