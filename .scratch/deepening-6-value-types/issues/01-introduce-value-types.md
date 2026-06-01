# 01: Introduce TodoId, TodoTitle, TodoTitleSearch Value Types

**Status:** completed

## Description

Create three `readonly record struct` value types in `BackendApi.Domain`:

### `TodoId`
- Wraps `Guid`
- `TodoId.New()` factory using `Guid.NewGuid()`
- Implements `IParsable<TodoId>` for ASP.NET Core route binding (`Parse`, `TryParse`)
- Implicit conversion to `Guid`
- `ToString()` returns `Value.ToString()` (for URL construction like `$"/todos/{todo.Id}"`)

### `TodoTitle`
- Wraps `string`
- Constructor validates: throws `ArgumentException` if null, empty, or whitespace
- Message: `"Title cannot be empty or null"`
- Implicit conversion to `string`

### `TodoTitleSearch`
- Wraps `string?`
- No validation — carries intent only

### Tests
- `TodoId`: `New()` produces non-empty, `Parse`/`TryParse` round-trip, equality, implicit to `Guid`
- `TodoTitle`: valid construction, empty/null/whitespace rejection, implicit to `string`
- `TodoTitleSearch`: no tests needed

### Prior Art
- `BackendApi.Tests\Domain\TodoTests.cs` for test patterns
- Value types live alongside `Todo.cs` in `BackendApi.Domain/`
