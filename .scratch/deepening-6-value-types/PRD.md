# PRD: Introduce DDD Value Types for Todo Domain

## Problem Statement

The `Todo` domain model uses primitive types (`Guid`, `string`, `DateTime`) for its properties, which means:

- The domain cannot enforce its own invariants at the type level — validation is duplicated across use cases and the domain constructor.
- A `Guid` used as an ID carries no semantic meaning; a `string` used as a title can be empty despite the invariant that says it cannot.
- The `Todo` record has a Dapper materialization constructor that bypasses validation, allowing invalid domain objects to exist in memory.
- `DateTime` lacks timezone information, making it ambiguous whether timestamps are UTC or local.

This creates ambiguity about where invariants live, risks inconsistent enforcement, and violates the principle that domain models should make invalid states unrepresentable.

## Solution

Introduce DDD value types (`TodoId`, `TodoTitle`, `TodoTitleSearch`) as `readonly record struct` types that encapsulate validation and intent. Remove the Dapper constructor from `Todo` — storage adapters define their own DTO models and map to/from domain models at repository boundaries. Change `CreatedAt` from `DateTime` to `DateTimeOffset`. Move all validation into the value types and the domain constructor. Remove duplicated validation from use cases. Endpoints construct value types from HTTP DTOs; use cases accept only domain types.

## User Stories

1. As a developer, I want `TodoId` to be a distinct type so that I cannot accidentally pass a random `Guid` where a `TodoId` is expected.
2. As a developer, I want `TodoTitle` to validate non-empty/non-whitespace in its constructor so that a `Todo` with an invalid title cannot exist.
3. As a developer, I want `TodoTitleSearch` to carry intent as a type so that search parameters are distinguishable from raw strings in the domain layer.
4. As a developer, I want the `Todo` domain model to have no persistence constructors so that all `Todo` instances are guaranteed valid.
5. As a developer, I want `CreatedAt` to be `DateTimeOffset` so that timestamps carry unambiguous timezone information.
6. As a developer, I want use cases to accept value types (not raw strings) so that the application layer contract is explicit about requiring validated input.
7. As a developer, I want endpoints to construct value types from HTTP DTOs so that validation exceptions bubble up to the existing error-handling middleware.
8. As a developer, I want the repository port to use `TodoId` instead of `Guid` so that the application layer speaks in domain terms.
9. As a developer, I want the Postgres adapter to use a private `TodoRow` DTO with primitives so that Dapper never touches domain types or value types.
10. As a developer, I want `TodoId` to parse from route strings automatically so that endpoint handlers receive `TodoId` without manual conversion.
11. As a developer, I want `TodoTitle` to implicitly convert to `string` so that mappers can extract the value without `.Value` noise.
12. As a developer, I want the `FakeTodoRepository` to use `Dictionary<TodoId, Todo>` so that test doubles compile and behave consistently with the real repository.
13. As a user, I want the API contract to remain unchanged (JSON shapes stay the same) so that the frontend is not affected by internal domain changes.
14. As a developer, I want domain tests to cover value type validation so that invariants are tested at their source of truth.
15. As a developer, I want use case tests to focus on orchestration (not validation) so that tests reflect the single responsibility of each layer.

## Implementation Decisions

### Value Type Shapes

All value types are `readonly record struct`:

```csharp
public readonly record struct TodoId(Guid Value) : IParsable<TodoId>
{
    public static TodoId New() => new(Guid.NewGuid());
    public static TodoId Parse(string s, IFormatProvider? provider) => new(Guid.Parse(s));
    public static bool TryParse(string? s, IFormatProvider? provider, out TodoId result) { ... }
    public static implicit operator Guid(TodoId id) => id.Value;
}

public readonly record struct TodoTitle(string Value)
{
    public TodoTitle(string value) : this(value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Title cannot be empty or null", nameof(value));
    }
    public static implicit operator string(TodoTitle title) => title.Value;
}

public readonly record struct TodoTitleSearch(string? Value);
```

### Domain Model Changes

- `Todo.Id` → `TodoId`
- `Todo.Title` → `TodoTitle`
- `Todo.CreatedAt` → `DateTimeOffset`
- `Todo.Completed` → stays `bool`
- Remove the Dapper materialization constructor entirely
- `Todo(Guid id, string title)` → `Todo(TodoId id, TodoTitle title)`
- `Todo.WithTitle(string)` → `Todo.WithTitle(TodoTitle)` — trusts the value type, no re-validation
- `Todo.ToggleCompleted()` unchanged

### Repository Port Changes

- `ITodoRepository.GetByIdAsync(Guid id, ...)` → `GetByIdAsync(TodoId id, ...)`
- `ITodoRepository.DeleteAsync(Guid id, ...)` → `DeleteAsync(TodoId id, ...)`
- `ITodoRepository.DeleteByIdsAsync(IEnumerable<Guid> ids, ...)` → `DeleteByIdsAsync(IEnumerable<TodoId> ids, ...)`
- `AddAsync`, `GetAllAsync`, `UpdateAsync` signatures unchanged (they already use `Todo`)

### Use Case Changes

- `CreateTodoUseCase.Execute(string title, ...)` → `Execute(TodoTitle title, ...)` — validation removed
- `UpdateTitleUseCase.Execute(Guid id, string newTitle, ...)` → `Execute(TodoId id, TodoTitle newTitle, ...)` — validation removed
- `DeleteTodoUseCase.Execute(Guid id, ...)` → `Execute(TodoId id, ...)`
- `BulkDeleteTodoUseCase.Execute(IEnumerable<Guid> ids, ...)` → `Execute(IEnumerable<TodoId> ids, ...)`
- `ToggleCompletedUseCase.Execute(Guid id, ...)` → `Execute(TodoId id, ...)`
- `ListTodosUseCase.Execute(bool? completed, string? search, ...)` → `Execute(bool? completed, TodoTitleSearch search, ...)`

### Endpoint Changes

- Endpoints construct value types from HTTP DTOs before calling use cases
- `app.MapPost("/todos", ...)` constructs `TodoTitle` from `request.Title`
- `app.MapPut("/todos/{id}/title", ...)` receives `TodoId` via `IParsable` binding, constructs `TodoTitle`
- `app.MapPut("/todos/{id}/toggle", ...)` receives `TodoId` via `IParsable` binding
- `app.MapDelete("/todos/{id}", ...)` receives `TodoId` via `IParsable` binding
- `app.MapGet("/todos", ...)` constructs `TodoTitleSearch` from query parameter
- `app.MapDelete("/todos", ...)` parses `ids` query to `IEnumerable<TodoId>`
- Existing `catch (ArgumentException)` blocks handle validation errors from value type constructors

### Storage Adapter Changes

- `PostgresTodoRepository` defines a private `TodoRow` DTO with primitive types (`Guid`, `string`, `bool`, `DateTimeOffset`)
- `TodoRow.FromDomain(Todo)` maps domain → primitives for writes
- `TodoRow.ToDomain()` maps primitives → domain (`TodoId`, `TodoTitle`, `DateTimeOffset`) for reads
- Dapper queries use `TodoRow`, never `Todo`
- Postgres table schema changes `created_at` from `TIMESTAMP` to `TIMESTAMPTZ`

### Mapper Changes

- `TodoMapper.ToResponse` extracts `todo.Id.Value` (→ `Guid`), `todo.Title` (implicit → `string`), `todo.CreatedAt` (→ `DateTimeOffset`)
- `TodoResponse.CreatedAt` changes from `DateTime` to `DateTimeOffset`
- `TodoResponse.Id` stays `Guid` (HTTP contract unchanged)
- `TodoResponse.Title` stays `string` (HTTP contract unchanged)

### Fake Repository Changes

- `FakeTodoRepository` internal storage: `Dictionary<Guid, Todo>` → `Dictionary<TodoId, Todo>`
- All method signatures updated to match `ITodoRepository`

### HTTP Contract

- No changes to JSON request/response shapes
- `TodoCreateRequest` and `TodoUpdateTitleRequest` stay as `string Title`
- `TodoResponse` stays `Guid Id, string Title, bool Completed, DateTimeOffset CreatedAt`
- Route parameters parse as `TodoId` via `IParsable` (transparent to clients)

## Testing Decisions

### What Makes a Good Test

Only test external behavior, not implementation details. Domain value types are tested for their invariants (validation, equality, parsing). Use case tests focus on orchestration (repository interactions) since validation lives in value types. Integration tests verify the full HTTP flow is unchanged.

### Modules to Test

| Module | Test Type | Prior Art |
|---|---|---|
| `TodoId` | Unit tests — `New()`, `Parse`, `TryParse`, equality, implicit to `Guid` | `BackendApi.Tests\Domain\TodoTests.cs` |
| `TodoTitle` | Unit tests — valid construction, empty/null/whitespace rejection, implicit to `string` | `BackendApi.Tests\Domain\TodoTests.cs` |
| `TodoTitleSearch` | No tests — no behavior, just a wrapper | N/A |
| `Todo` | Update existing domain tests to use value types; verify invariants still hold | `BackendApi.Tests\Domain\TodoTests.cs` |
| `CreateTodoUseCase` | Remove validation tests (`WithEmptyTitle_Throws`, `WithNullTitle_Throws`); keep orchestration test (`SavesToRepository`) | `BackendApi.Tests\Application\UseCases\CreateTodoUseCaseTests.cs` |
| `UpdateTitleUseCase` | Remove validation tests; keep orchestration tests | `BackendApi.Tests\Application\UseCases\UpdateTitleUseCaseTests.cs` |
| `FakeTodoRepository` | No new tests — just update to compile | `BackendApi.Tests\TestDoubles\FakeTodoRepository.cs` |
| `PostgresTodoRepository` | Update existing integration tests to use value types; verify `TIMESTAMPTZ` mapping | `BackendApi.Tests\Storage\Postgres\PostgresTodoRepositoryTests.cs` |
| `TodoApiIntegrationTests` | Run as-is — HTTP contract unchanged; verifies end-to-end flow | `BackendApi.Tests\TodoApiIntegrationTests.cs` |

## Out of Scope

- Removing identity pass-through use cases (`DeleteTodoUseCase`, `BulkDeleteTodoUseCase`) — keep them, just update signatures
- Adding clock or ID generator ports — `TodoId.New()` uses `Guid.NewGuid()` directly
- Adding a `JsonConverter<TodoId>` — the mapper handles conversion to `Guid` for JSON serialization
- Changing the HTTP API contract — JSON shapes remain identical
- Frontend changes — the React app is unaffected
- Adding max-length or other title validation rules — `TodoTitle` enforces non-empty only; additional rules can be added later

## Further Notes

- This deepening consolidates validation logic (Deepening 6) and introduces DDD value types in a single pass, since the two concerns are tightly coupled — the value types are where validation lives.
- The `TodoTitleSearch` type carries intent but has no validation. It exists so the domain layer speaks in its own vocabulary rather than raw `string?`.
- `IParsable<TodoId>` enables ASP.NET Core minimal APIs to bind route parameters directly to `TodoId` without manual parsing in endpoint handlers.
- The Postgres schema change (`TIMESTAMP` → `TIMESTAMPTZ`) is a breaking change for existing data, but this is a test/dev repo so migration is a simple table recreate.
- All existing integration tests should pass without modification to their assertions, since the HTTP contract is unchanged.
