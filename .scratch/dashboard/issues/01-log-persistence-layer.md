Status: ready-for-agent

## Parent

- `.scratch/dashboard/PRD.md`

## What to build

Create the log persistence layer — the foundation for all observability features. This includes the database schema, the application-layer port, the PostgreSQL implementation, and tests.

**Database**: Add migration `002-create-logs-table.sql` with the following schema:

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

**Application layer**: Create `ILogStore` port with:
- `WriteAsync(LogEntry entry)` — writes a single log entry
- `QueryAsync(LogFilter filter)` — returns filtered log entries ordered by timestamp descending

Create `LogEntry` and `LogFilter` records. `LogFilter` should support optional `level` and `message` filtering.

**Storage layer**: Create `PostgresLogStore` implementing `ILogStore` using Dapper + Npgsql, registered via a `AddLogStore()` DI extension method.

**Tests**: Integration tests following the pattern in `PostgresTodoRepositoryTests`:
- Write a log entry, query it back
- Verify ordering by timestamp descending
- Verify filtering by level (info, warning, error)
- Verify filtering by message text (partial match)
- Verify combined filters work
- Verify empty results when no entries match

## Acceptance criteria

- [ ] Migration `002-create-logs-table.sql` exists and runs on startup
- [ ] `ILogStore` port exists in the application layer with `WriteAsync` and `QueryAsync`
- [ ] `LogEntry` and `LogFilter` records exist with appropriate fields
- [ ] `PostgresLogStore` implements `ILogStore` using Dapper + Npgsql
- [ ] DI registration via extension method, wired in `Program.cs`
- [ ] Integration tests cover write, query, ordering, filtering, and empty results
- [ ] All existing tests still pass

## Blocked by

None - can start immediately
