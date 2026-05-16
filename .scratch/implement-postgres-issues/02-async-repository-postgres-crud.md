Status: done

# 02-async-repository-postgres-crud

## Parent

[.scratch/implement-postgres-PRD.md](../implement-postgres-PRD.md)

## What to build

Update `ITodoRepository` in `BackendApi.Application.Ports` so all methods are async: `AddAsync`, `GetByIdAsync`, `GetAllAsync`, `UpdateAsync`, `DeleteAsync`, `DeleteByIdsAsync`, each accepting `CancellationToken ct = default`. Implement `AddAsync`, `GetByIdAsync`, and `GetAllAsync` in `PostgresTodoRepository` using Dapper queries against the `todos` table. Update `CreateTodoUseCase` and `ListTodosUseCase` to await the async repository calls (their `Execute` methods become `async Task<Todo>` and `async Task<IReadOnlyList<Todo>>`). Update `ListTodosUseCase` to pass `completed` filter and `search` filter down to the repository for SQL-level filtering. Wire DI in `Program.cs` to call `AddPostgresStorage()` instead of `AddInfrastructure()`. Add Testcontainers-based integration tests for the repository's create and read operations. Update existing unit tests in `BackendApi.Tests/Application/` to use a mock or in-memory test double for `ITodoRepository` instead of `InMemoryTodoRepository`.

## Acceptance criteria

- [ ] `ITodoRepository` has all async method signatures with CancellationToken
- [ ] `PostgresTodoRepository.AddAsync` inserts a todo into PostgreSQL
- [ ] `PostgresTodoRepository.GetByIdAsync` returns a todo or null
- [ ] `PostgresTodoRepository.GetAllAsync` returns all todos with optional completed/search filters applied at SQL level
- [ ] `CreateTodoUseCase.Execute` is async and awaits repository
- [ ] `ListTodosUseCase.Execute` is async and awaits repository with filter support
- [ ] `Program.cs` uses `AddPostgresStorage()` instead of `AddInfrastructure()`
- [ ] Integration tests verify create/read against Testcontainers.PostgreSql
- [ ] Existing use case unit tests pass with mock repository
- [ ] `POST /todos` returns 201 with persisted data
- [ ] `GET /todos` returns persisted todos with filter/search support

## Blocked by

- [#01-postgres-foundation-migration-runner](01-postgres-foundation-migration-runner.md)
