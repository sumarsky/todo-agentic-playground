# 04: Update Use Cases — Remove Validation, Accept Value Types

**Status:** completed

## Description

Update all use cases in `BackendApi.Application/UseCases` to accept value types and remove duplicated validation.

### `CreateTodoUseCase`
- Signature: `Execute(TodoTitle title, CancellationToken ct = default)`
- Remove `if (string.IsNullOrWhiteSpace(title))` check — `TodoTitle` validates
- Construct `Todo` with `new Todo(TodoId.New(), title)`

### `UpdateTitleUseCase`
- Signature: `Execute(TodoId id, TodoTitle newTitle, CancellationToken ct = default)`
- Remove `if (string.IsNullOrWhiteSpace(newTitle))` check — `TodoTitle` validates
- Call `todo.WithTitle(newTitle)` — trusts value type

### `DeleteTodoUseCase`
- Signature: `Execute(TodoId id, CancellationToken ct = default)`

### `BulkDeleteTodoUseCase`
- Signature: `Execute(IEnumerable<TodoId> ids, CancellationToken ct = default)`

### `ToggleCompletedUseCase`
- Signature: `Execute(TodoId id, CancellationToken ct = default)`

### `ListTodosUseCase`
- Signature: `Execute(bool? completed, TodoTitleSearch search, CancellationToken ct = default)`
- Pass `search.Value` to repository (repository handles filtering)

### Tests
- `CreateTodoUseCaseTests`: remove `WithEmptyTitle_ThrowsArgumentException` and `WithNullTitle_ThrowsArgumentException`; keep `WithValidTitle_ReturnsTodoWithId` and `SavesToRepository`; update `Assert.NotEqual(Guid.Empty, result.Id)` → `Assert.NotEqual(TodoId.New(), result.Id)` or just `Assert.NotEqual(default, result.Id)`
- `UpdateTitleUseCaseTests`: remove `WithEmptyTitle_ThrowsArgumentException`; keep `WithValidTitle_UpdatesTodo`, `SavesChangesToRepository`, `TodoNotFound_ReturnsNull`; replace `Guid.NewGuid()` with `TodoId.New()` in test code
- `ToggleCompletedUseCaseTests`: replace `Guid.NewGuid()` with `TodoId.New()` in `TodoNotFound_ReturnsNull`; other tests use `todo.Id` which is now `TodoId` — no change needed
- Other use case tests: update signatures, no behavioral changes expected

### Prior Art
- `BackendApi.Tests\Application\UseCases\CreateTodoUseCaseTests.cs`
- `BackendApi.Tests\Application\UseCases\UpdateTitleUseCaseTests.cs`
- `BackendApi.Tests\Application\UseCases\ToggleCompletedUseCaseTests.cs`
