# 03: Update ITodoRepository Port and FakeTodoRepository

**Status:** completed

## Description

Update the repository port and its in-memory test double to use `TodoId`.

### `ITodoRepository` (BackendApi.Application/Ports)
- `GetByIdAsync(Guid id, ...)` → `GetByIdAsync(TodoId id, ...)`
- `DeleteAsync(Guid id, ...)` → `DeleteAsync(TodoId id, ...)`
- `DeleteByIdsAsync(IEnumerable<Guid> ids, ...)` → `DeleteByIdsAsync(IEnumerable<TodoId> ids, ...)`
- `AddAsync`, `GetAllAsync`, `UpdateAsync` unchanged (already use `Todo`)

### `FakeTodoRepository` (BackendApi.Tests/TestDoubles)
- Internal storage: `Dictionary<Guid, Todo>` → `Dictionary<TodoId, Todo>`
- All method signatures updated to match `ITodoRepository`
- Search filter in `GetAllAsync` uses `TodoTitle.Value` for string comparison

### Tests
- No new tests for `FakeTodoRepository` — just verify it compiles and existing tests pass
- Existing use case and integration tests will exercise the fake through the updated port

### Prior Art
- `BackendApi.Application/Ports/ITodoRepository.cs`
- `BackendApi.Tests/TestDoubles/FakeTodoRepository.cs`
