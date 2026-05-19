# Deepening Opportunity 3: Remove Identity Pass-Through Use Cases

**Severity:** 🔴 CRITICAL  
**Status:** Not started  
**Complexity:** Low

## Problem Statement

Several use cases are thin wrappers around repository calls with **no business logic**. They add indirection without providing leverage or locality — violations of the deletion test.

### Current State

**Files:**
- `BackendApi.Application/UseCases/DeleteTodoUseCase.cs` (18 lines)
- `BackendApi.Application/UseCases/ListTodosUseCase.cs` (20 lines)
- `BackendApi.Application/UseCases/BulkDeleteTodoUseCase.cs` (15 lines)

### DeleteTodoUseCase

```csharp
namespace BackendApi.Application.UseCases;

public class DeleteTodoUseCase
{
    private readonly ITodoRepository _repository;

    public DeleteTodoUseCase(ITodoRepository repository) => _repository = repository;

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
        => _repository.DeleteAsync(id, ct);
}
```

**Analysis:**
- **Interface complexity:** Public method signature is `DeleteAsync(Guid id, CancellationToken ct)`
- **Implementation complexity:** Direct pass-through to `_repository.DeleteAsync(id, ct)`
- **Deletion test:** If you deleted this class, would complexity vanish or reappear? → **Reappears nowhere.** No caller depends on this wrapper; they could call repo directly.
- **Leverage:** None. No business logic, no orchestration, no error handling beyond what repo does.
- **Locality:** No. Change is required in two places (use case and repo) if behavior needs to shift.

### ListTodosUseCase

```csharp
namespace BackendApi.Application.UseCases;

public class ListTodosUseCase
{
    private readonly ITodoRepository _repository;

    public ListTodosUseCase(ITodoRepository repository) => _repository = repository;

    public Task<IReadOnlyList<Todo>> ExecuteAsync(
        bool? completed = null, 
        string? search = null, 
        CancellationToken ct = default)
        => _repository.GetAllAsync(completed, search, ct);
}
```

**Analysis:**
- Same problem: pure pass-through, no business logic
- Repository already abstracts persistence; use case adds no further abstraction

### BulkDeleteTodoUseCase

```csharp
namespace BackendApi.Application.UseCases;

public class BulkDeleteTodoUseCase
{
    private readonly ITodoRepository _repository;

    public BulkDeleteTodoUseCase(ITodoRepository repository) => _repository = repository;

    public Task BulkDeleteAsync(IReadOnlyList<Guid> ids, CancellationToken ct = default)
        => _repository.BulkDeleteAsync(ids, ct);
}
```

**Same issue:** thin wrapper, no value added.

### Why This Matters

- **Indirection without leverage:** Callers must traverse use case → repository to understand data flow
- **Maintenance burden:** Three files to maintain for what is essentially boilerplate
- **Testing cost:** Tests must verify both use case and repository for the same behavior
- **Future friction:** When real domain logic is needed, it's unclear where to add it (use case? repo? new layer?)

## Solution

Delete the identity pass-through use cases and call the repository directly from endpoints.

### Refactored Endpoints

**File:** `BackendApi/Program.cs`

#### Before

```csharp
app.MapDelete("/todos/{id}", async (Guid id, DeleteTodoUseCase useCase, CancellationToken ct) =>
{
    await useCase.DeleteAsync(id, ct);
    return Results.NoContent();
});
```

#### After

```csharp
app.MapDelete("/todos/{id}", async (Guid id, ITodoRepository repository, CancellationToken ct) =>
{
    await repository.DeleteAsync(id, ct);
    return Results.NoContent();
});
```

#### Before (List)

```csharp
app.MapGet("/todos", async (bool? completed, string? search, ListTodosUseCase useCase, CancellationToken ct) =>
{
    var todos = await useCase.ExecuteAsync(completed, search, ct);
    var response = todos.Select(TodoMapper.ToResponse).ToList();
    return Results.Ok(response);
});
```

#### After

```csharp
app.MapGet("/todos", async (bool? completed, string? search, ITodoRepository repository, CancellationToken ct) =>
{
    var todos = await repository.GetAllAsync(completed, search, ct);
    var response = todos.Select(TodoMapper.ToResponse).ToList();
    return Results.Ok(response);
});
```

#### Before (Bulk Delete)

```csharp
app.MapDelete("/todos", async (HttpContext context, BulkDeleteTodoUseCase useCase, CancellationToken ct) =>
{
    var ids = /* parse IDs from query string */;
    await useCase.BulkDeleteAsync(ids, ct);
    return Results.NoContent();
});
```

#### After

```csharp
app.MapDelete("/todos", async (HttpContext context, ITodoRepository repository, CancellationToken ct) =>
{
    var ids = /* parse IDs from query string */;
    await repository.BulkDeleteAsync(ids, ct);
    return Results.NoContent();
});
```

## Benefits

### Locality
- **Direct mapping:** Endpoint behavior maps 1:1 to repository contract; no indirection
- **Clear ownership:** If behavior needs to change, edit endpoint or repository—not both
- **Less code:** Remove three files, simplify dependency injection

### Leverage
- **Simpler architecture:** Clear two-layer stack (endpoint → repository) instead of three-layer
- **Easier onboarding:** New developers see repository interface directly; no guessing about use case purpose
- **Flexible future growth:** When domain logic is needed (e.g., validation, enrichment), add it at the right layer

### Testing

**Before:** Tests must verify both use case and repository
```csharp
[Fact]
public async Task DeletesTodo()
{
    var mockRepo = new Mock<ITodoRepository>();
    var useCase = new DeleteTodoUseCase(mockRepo.Object);
    
    await useCase.DeleteAsync(Guid.NewGuid());
    
    mockRepo.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), default));
}

[Fact]
public async Task EndpointCallsUseCase()
{
    var mockUseCase = new Mock<DeleteTodoUseCase>();
    
    // Must mock use case AND test endpoint
    // Brittle due to multiple layers
}
```

**After:** Test endpoint directly against repository
```csharp
[Fact]
public async Task EndpointDeletesTodo()
{
    var mockRepo = new Mock<ITodoRepository>();
    var client = new HttpClient { /* configured with mock */ };
    
    // Direct call: endpoint → repo, no middleman
    var response = await client.DeleteAsync("/todos/123");
    
    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    mockRepo.Verify(r => r.DeleteAsync(Guid.Parse("123"), default));
}
```

## Implementation Checklist

- [ ] Remove `BackendApi.Application/UseCases/DeleteTodoUseCase.cs`
- [ ] Remove `BackendApi.Application/UseCases/ListTodosUseCase.cs`
- [ ] Remove `BackendApi.Application/UseCases/BulkDeleteTodoUseCase.cs`
- [ ] Update `BackendApi/Program.cs` endpoints to inject `ITodoRepository` directly
- [ ] Remove use case registrations from dependency injection
- [ ] Update related unit tests
- [ ] Verify all endpoints still work

## Decision: When NOT to Delete Use Cases

Use cases should remain if they provide **real business logic**:

**Keep:**
- `CreateTodoUseCase` — even though it's thin, it centralizes todo creation (good seam for future logic)
- `UpdateTodoUseCase` — handles multiple update paths (title vs. completed)

**Delete:**
- `DeleteTodoUseCase` — no domain logic, direct pass-through
- `ListTodosUseCase` — no domain logic, direct pass-through
- `BulkDeleteTodoUseCase` — no domain logic, direct pass-through

**Future rule:** Use cases stay if they encapsulate business invariants or orchestration. Remove if they're thin wrappers.

## Related Deepening Opportunities

- **Deepening 2:** Hoist Metrics Logic to Use Case (shows use case done right: real domain logic inside)
- **Deepening 6:** Consolidate Validation Logic in Domain (related pattern: push logic to domain, not use cases)
