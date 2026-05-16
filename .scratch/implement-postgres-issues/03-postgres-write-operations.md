Status: done

# 03-postgres-write-operations

## Parent

[.scratch/implement-postgres-PRD.md](../implement-postgres-PRD.md)

## What to build

Complete the `PostgresTodoRepository` implementation by adding `UpdateAsync`, `DeleteAsync`, and `DeleteByIdsAsync` using Dapper queries. Update `UpdateTitleUseCase`, `ToggleCompletedUseCase`, `DeleteTodoUseCase`, and `BulkDeleteTodoUseCase` to await the async repository calls (their `Execute` methods become async where they aren't already). Add Testcontainers-based integration tests covering update, toggle, single delete, and bulk delete operations against a real PostgreSQL instance. Ensure all existing API integration tests pass against the Postgres-backed repository.

## Acceptance criteria

- [ ] `PostgresTodoRepository.UpdateAsync` persists todo changes to PostgreSQL
- [ ] `PostgresTodoRepository.DeleteAsync` removes a single todo by id
- [ ] `PostgresTodoRepository.DeleteByIdsAsync` removes multiple todos by ids
- [ ] `UpdateTitleUseCase.Execute` is async and returns 404-equivalent for not-found
- [ ] `ToggleCompletedUseCase.Execute` is async and returns 404-equivalent for not-found
- [ ] `DeleteTodoUseCase.Execute` is async
- [ ] `BulkDeleteTodoUseCase.Execute` is async
- [ ] Integration tests verify update/toggle/delete/bulk-delete against Testcontainers.PostgreSql
- [ ] All existing API integration tests pass with Postgres backend
- [ ] `PUT /todos/{id}/title` persists changes
- [ ] `PUT /todos/{id}/toggle` persists changes
- [ ] `DELETE /todos/{id}` removes the todo
- [ ] `DELETE /todos?ids=...` removes specified todos

## Blocked by

- [#02-async-repository-postgres-crud](02-async-repository-postgres-crud.md)
