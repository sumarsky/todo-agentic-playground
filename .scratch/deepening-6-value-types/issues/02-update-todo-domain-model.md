# 02: Update Todo Domain Model to Use Value Types

**Status:** completed

## Description

Update `Todo` in `BackendApi.Domain` to use the new value types from issue #01:

### Property Changes
- `Id`: `Guid` → `TodoId`
- `Title`: `string` → `TodoTitle`
- `CreatedAt`: `DateTime` → `DateTimeOffset`
- `Completed`: stays `bool`

### Constructor Changes
- Primary constructor: `Todo(TodoId id, TodoTitle title)` — sets `Completed = false`, `CreatedAt = DateTimeOffset.UtcNow`
- Remove the Dapper materialization constructor entirely (no second constructor)

### Method Changes
- `ToggleCompleted()` — unchanged (uses `this with`)
- `WithTitle(TodoTitle newTitle)` — accepts `TodoTitle`, trusts validation, no re-validation

### Tests
- Update existing `TodoTests.cs` to use `TodoId.New()`, `new TodoTitle("...")`, `DateTimeOffset`
- Verify all existing invariants still hold (title validation, immutability, toggle behavior)
- Verify `WithTitle` preserves identity, completion state, and creation time

### Prior Art
- `BackendApi.Tests\Domain\TodoTests.cs`
