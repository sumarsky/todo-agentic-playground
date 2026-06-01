# 05: Update Postgres Adapter — TodoRow DTO and TIMESTAMPTZ

**Status:** completed

## Description

Update `PostgresTodoRepository` to use a private DTO for Dapper mapping, never touching domain types directly.

### Private `TodoRow` DTO (inside `PostgresTodoRepository`)
```csharp
private readonly record struct TodoRow(
    Guid Id,
    string Title,
    bool Completed,
    DateTimeOffset CreatedAt);
```

### Mapping Methods
- `static TodoRow FromDomain(Todo todo)` — extracts `.Value` from `TodoId`, `TodoTitle`, uses `CreatedAt` directly
- `Todo ToDomain()` — constructs `new Todo(new TodoId(Id), new TodoTitle(Title), Completed, CreatedAt)`

### Query Changes
- All Dapper `QueryAsync<Todo>` → `QueryAsync<TodoRow>` then `.Select(r => r.ToDomain())`
- All Dapper `ExecuteAsync` with `new { todo.Id, ... }` → `TodoRow.FromDomain(todo)`

### Schema Change
- Migration `001-create-todos-table.sql` already uses `timestamp with time zone` — no change needed
- Update test table creation in `PostgresTodoRepositoryTests.cs`: `created_at TIMESTAMP` → `created_at TIMESTAMPTZ` to match the migration

### Tests
- Update `PostgresTodoRepositoryTests.cs` to use `TodoId.New()`, `new TodoTitle("...")`, `DateTimeOffset`
- Verify `TIMESTAMPTZ` mapping preserves timezone info
- All existing assertions should pass with updated value type construction

### Prior Art
- `BackendApi.Storage.Postgres/PostgresTodoRepository.cs`
- `BackendApi.Tests\Storage\Postgres\PostgresTodoRepositoryTests.cs`
