Status: ready-for-agent

# PRD: Swap In-Memory Storage for PostgreSQL

## Problem Statement

As a developer, I want to persist todos in PostgreSQL instead of an in-memory dictionary, so that data survives application restarts and the storage layer is production-ready.

## Solution

Replace the `InMemoryTodoRepository` with a Dapper-based `PostgresTodoRepository` in a new `BackendApi.Storage.Postgres` project. Update the repository interface to async, wire up connection configuration, and add a lightweight migration runner that applies idempotent SQL scripts at startup.

## User Stories

1. As an application operator, I want the app to connect to a PostgreSQL database via a configurable connection string, so that I can deploy to any environment.
2. As a developer, I want the `todos` table to be created automatically on startup using idempotent SQL scripts, so that I don't need to run manual migrations.
3. As a user, I want my todos to persist across application restarts, so that I don't lose my data.
4. As a user, I want to create a todo via `POST /todos` and have it stored in PostgreSQL, so that it is durable.
5. As a user, I want to list all todos via `GET /todos` and have them read from PostgreSQL, so that I see persisted data.
6. As a user, I want to filter todos by completion state and search by title via query params, and have the filtering happen in PostgreSQL, so that it scales with data volume.
7. As a user, I want to update a todo's title via `PUT /todos/{id}/title` and have the change persisted in PostgreSQL.
8. As a user, I want to toggle a todo's completion via `PUT /todos/{id}/toggle` and have the change persisted in PostgreSQL.
9. As a user, I want to delete a single todo via `DELETE /todos/{id}` and have it removed from PostgreSQL.
10. As a user, I want to bulk delete todos via `DELETE /todos?ids=...` and have them removed from PostgreSQL.
11. As a developer, I want repository methods to be async, so that database I/O doesn't block thread pool threads.
12. As a developer, I want integration tests to spin up a Testcontainers PostgreSQL instance and run migrations, so that I can verify the repository against a real database.
13. As a developer, I want unit tests to use a mock or test double for `ITodoRepository`, so that use case logic can be tested without a database.
14. As a developer, I want the `BackendApi.Infrastructure` project removed, so that there's no confusion about which storage implementation is active.

## Implementation Decisions

### Modules to build/modify

| Module | Action |
|---|---|
| `BackendApi.Storage.Postgres` (new) | Dapper-based `PostgresTodoRepository`, migration runner, DI extension, `Migrations/` folder |
| `ITodoRepository` (modify) | All methods become async: `AddAsync`, `GetByIdAsync`, `GetAllAsync`, `UpdateAsync`, `DeleteAsync`, `DeleteByIdsAsync` |
| Use cases (modify) | All use cases await async repository calls |
| API endpoints (modify) | Endpoint handlers become `async`, DI wiring swaps to Postgres |
| `BackendApi.Infrastructure` (remove) | Delete project, remove from solution and test references |
| `BackendApi.Tests` (modify) | Add Testcontainers-based integration tests, update unit tests to use mocks |

### `ITodoRepository` interface (async)

```csharp
public interface ITodoRepository
{
    Task AddAsync(Todo todo, CancellationToken ct = default);
    Task<Todo?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Todo>> GetAllAsync(CancellationToken ct = default);
    Task UpdateAsync(Todo todo, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task DeleteByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
}
```

### Connection lifecycle

- `NpgsqlDataSource` registered as a singleton via DI.
- Injected into `PostgresTodoRepository`.
- Each repository method calls `dataSource.CreateConnection()`, opens it, executes the Dapper query, and disposes the connection at method end.
- Connection pooling handled by Npgsql automatically.

### Connection string configuration

- Read from `appsettings.json` under `ConnectionStrings:Default`.
- Supports environment variable override (`ConnectionStrings__Default`).

### Schema

Table: `todos`

| Column | Type | Constraints |
|---|---|---|
| `id` | uuid | PRIMARY KEY |
| `title` | text | NOT NULL |
| `completed` | boolean | NOT NULL, DEFAULT false |
| `created_at` | timestamp with time zone | NOT NULL |

### Migration runner

- Lives in `BackendApi.Storage.Postgres`.
- Reads all `*.sql` files from `Migrations/` folder, sorted by filename.
- Executes each script on startup via `NpgsqlConnection`.
- All scripts are idempotent (use `CREATE TABLE IF NOT EXISTS`, etc.).
- No tracking table needed.

### Error handling

- `NpgsqlException` caught at repository boundary for known cases (e.g., not-found scenarios).
- Unexpected database errors bubble up to the global error handler → 500 response.
- Use cases handle "not found" by throwing/returning appropriate 404 responses.

### NuGet packages

- `BackendApi.Storage.Postgres`: `Dapper`, `Npgsql`
- `BackendApi.Tests`: `Testcontainers.PostgreSql`

### DI registration

- New extension method `AddPostgresStorage(this IServiceCollection services, IConfiguration configuration)` in `BackendApi.Storage.Postgres`.
- Called from `Program.cs` in place of `AddInfrastructure()`.

## Testing Decisions

### What makes a good test

Only test external behavior, not implementation details. Tests should verify that the repository correctly persists and retrieves `Todo` entities, and that use cases orchestrate repository calls correctly.

### Modules to test

| Module | Test type | Prior art |
|---|---|---|
| `PostgresTodoRepository` | Integration (Testcontainers) | Existing `InMemoryTodoRepository` tests in `BackendApi.Tests/Infrastructure/` |
| Use cases | Unit (mock repository) | Existing use case tests in `BackendApi.Tests/Application/` |
| API endpoints | Integration (Testcontainers) | Existing API integration tests in `BackendApi.Tests/Api/` |
| Migration runner | Integration (Testcontainers) | New — verify table exists after startup |

### Test infrastructure

- Integration tests use `Testcontainers.PostgreSql` to spin up a temporary PostgreSQL instance.
- Migrations are executed against the test container before each test class/suite.
- Unit tests use a mock or in-memory test double for `ITodoRepository`.

## Out of Scope

- Adding a clock abstraction or ID generator port (per existing ADR).
- Adding repository interface abstraction beyond `ITodoRepository`.
- Changing the API contract or HTTP DTOs.
- Frontend changes — the React app is unaffected.
- Read replicas, connection retry policies, or advanced Npgsql configuration.
- Keeping the in-memory repository as a coexisting option.

## Further Notes

- The `Todo` domain model remains unchanged — immutability and invariants are preserved.
- The clean architecture boundaries remain: domain → application ports → storage adapter.
- All existing API behavior (filtering, searching, bulk delete) must continue to work identically.
