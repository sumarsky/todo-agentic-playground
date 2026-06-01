# 06: Update Endpoints and TodoMapper

**Status:** completed

## Description

Update HTTP endpoints in `Program.cs` and the `TodoMapper` to construct value types from DTOs and extract values for responses.

### Endpoint Changes

**`POST /todos`**
- Construct `var title = new TodoTitle(request.Title)` before calling use case
- `catch (ArgumentException)` already handles validation errors

**`PUT /todos/{id}/title`**
- Route parameter `Guid id` → `TodoId id` (bound via `IParsable`)
- Construct `var newTitle = new TodoTitle(request.Title)` before calling use case

**`PUT /todos/{id}/toggle`**
- Route parameter `Guid id` → `TodoId id`

**`DELETE /todos/{id}`**
- Route parameter `Guid id` → `TodoId id`

**`GET /todos`**
- Construct `var search = new TodoTitleSearch(search)` before calling use case

**`DELETE /todos?ids=...`**
- Parse IDs to `IEnumerable<TodoId>` instead of `IEnumerable<Guid>`

### `TodoMapper` Changes
- `ToResponse`: extract `todo.Id.Value` (→ `Guid`), `todo.Title` (implicit → `string`), `todo.CreatedAt` (→ `DateTimeOffset`)

### `TodoResponse` Contract Change
- `CreatedAt`: `DateTime` → `DateTimeOffset`
- `Id`: stays `Guid`
- `Title`: stays `string`

### Tests
- No new unit tests for mapper — integration tests cover the full flow
- `TodoApiIntegrationTests.cs` should pass without assertion changes (HTTP contract unchanged except `CreatedAt` format)

### Prior Art
- `BackendApi/Program.cs`
- `BackendApi/Mappers/TodoMapper.cs`
- `BackendApi/Contracts/TodoResponse.cs`
- `BackendApi.Tests/TodoApiIntegrationTests.cs`
