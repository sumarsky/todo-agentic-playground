# Deepening Opportunity 6: Consolidate Validation Logic in Domain

**Severity:** 🟡 MEDIUM  
**Status:** Not started  
**Complexity:** Low

## Problem Statement

Title validation logic is duplicated across two layers: the domain model (`Todo.cs`) and the application layer (`CreateTodoUseCase.cs`). This creates ambiguity about where invariants live and risks inconsistent enforcement.

### Current State

**File:** `BackendApi.Domain/Todo.cs`

```csharp
public record Todo
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required bool Completed { get; init; }
    public required DateTime CreatedAt { get; init; }

    // ← Constructor with validation
    public Todo(Guid id, string title, bool completed = false, DateTime? createdAt = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty or null", nameof(title));

        Id = id;
        Title = title;
        Completed = completed;
        CreatedAt = createdAt ?? DateTime.UtcNow;
    }

    public Todo WithTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty or null", nameof(title));

        return new Todo(Id, title, Completed, CreatedAt);
    }

    public Todo ToggleCompleted() => new(Id, Title, !Completed, CreatedAt);
}
```

**File:** `BackendApi.Application/UseCases/CreateTodoUseCase.cs`

```csharp
public class CreateTodoUseCase
{
    private readonly ITodoRepository _repository;

    public CreateTodoUseCase(ITodoRepository repository) => _repository = repository;

    public async Task<Todo> Execute(string title, CancellationToken ct = default)
    {
        // ← Validation ALSO here
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty or null", nameof(title));

        var todo = new Todo(Guid.NewGuid(), title);
        await _repository.AddAsync(todo, ct);
        return todo;
    }
}
```

### Why This Is Shallow

- **Validation lives in two places:** Domain constructor AND use case
- **Ambiguity:** Which validation is authoritative? If they diverge, which is correct?
- **Maintenance burden:** Change title rules → must update both places
- **Scattered knowledge:** Developer must understand both layers to grasp full invariant

### The Deletion Test

**Question:** If you deleted the use case validation, would the system break?
- **Answer:** No. The domain constructor still validates. Deleting use case validation changes nothing.
- **Implication:** Use case validation is redundant; domain already protects invariants.

### Real-World Scenario: What Happens When Rules Change

Imagine title validation must now be:
- Non-empty
- Maximum 255 characters
- No leading/trailing whitespace

**With duplication:**
```
1. Update Todo.cs constructor
2. Update Todo.WithTitle() method
3. Update CreateTodoUseCase.Execute()
4. Update UpdateTodoUseCase.Execute() (if it exists)
5. Update any other layer checking title
6. Tests might pass even if you miss one place
```

**Result:** Inconsistent validation across the app; bugs slip through.

## Solution

Remove validation from use case; let domain model enforce invariants. Trust that the domain constructor will validate.

### Refactored Domain Model

**File:** `BackendApi.Domain/Todo.cs` (no changes)

```csharp
public record Todo
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required bool Completed { get; init; }
    public required DateTime CreatedAt { get; init; }

    public Todo(Guid id, string title, bool completed = false, DateTime? createdAt = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty or null", nameof(title));

        Id = id;
        Title = title;
        Completed = completed;
        CreatedAt = createdAt ?? DateTime.UtcNow;
    }

    public Todo WithTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty or null", nameof(title));

        return new Todo(Id, title, Completed, CreatedAt);
    }

    public Todo ToggleCompleted() => new(Id, Title, !Completed, CreatedAt);
}
```

### Refactored Use Case

**File:** `BackendApi.Application/UseCases/CreateTodoUseCase.cs`

```csharp
public class CreateTodoUseCase
{
    private readonly ITodoRepository _repository;

    public CreateTodoUseCase(ITodoRepository repository) => _repository = repository;

    public async Task<Todo> Execute(string title, CancellationToken ct = default)
    {
        // ← Validation removed; Todo constructor handles it
        // If title is invalid, ArgumentException propagates from domain
        
        var todo = new Todo(Guid.NewGuid(), title);
        await _repository.AddAsync(todo, ct);
        return todo;
    }
}
```

### Endpoint Error Handling

**File:** `BackendApi/Program.cs`

Ensure the endpoint maps domain exceptions to HTTP errors:

```csharp
app.MapPost("/todos", async (CreateTodoRequest request, CreateTodoUseCase useCase, CancellationToken ct) =>
{
    try
    {
        var todo = await useCase.Execute(request.Title, ct);
        var response = TodoMapper.ToResponse(todo);
        return Results.CreatedAtRoute("GetTodo", new { id = todo.Id }, response);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new ErrorResponse(400, ex.Message));
    }
    catch (Exception ex)
    {
        // Log exception...
        return Results.InternalServerError();
    }
});
```

## Benefits

### Locality
- **Validation concentrated:** Single source of truth for title rules (domain constructor)
- **Change efficiency:** Add max-length rule → update one place (domain constructor)
- **Clear ownership:** Domain owns invariants; use case orchestrates

### Leverage
- **Reusable validation:** Any code creating Todo instances (API, background jobs, tests) enforces same rules
- **Type safety:** Compiler ensures `Todo` instances are always valid (no invalid state possible)
- **Consistent behavior:** Whether Title is validated in use case or directly in domain, the rule is the same

### Testing

**Before:** Tests must validate both layers
```csharp
[Fact]
public async Task CreateTodoValidatesTitleInUseCase()
{
    var useCase = new CreateTodoUseCase(mockRepository.Object);
    
    await Assert.ThrowsAsync<ArgumentException>(() => 
        useCase.Execute("", default));
}

[Fact]
public void TodoValidatesTitleInConstructor()
{
    Assert.Throws<ArgumentException>(() => 
        new Todo(Guid.NewGuid(), ""));
}
```

**After:** Validation tested once in domain; use case tests focus on orchestration
```csharp
[Fact]
public void TodoValidatesTitleInConstructor()
{
    // ← This is the one place validation is tested
    Assert.Throws<ArgumentException>(() => 
        new Todo(Guid.NewGuid(), ""));
}

[Fact]
public async Task CreateTodoOrchestrates()
{
    // Use case test focuses on repo interaction, not validation
    var mockRepository = new Mock<ITodoRepository>();
    var useCase = new CreateTodoUseCase(mockRepository.Object);
    
    var result = await useCase.Execute("Valid Title", default);
    
    mockRepository.Verify(r => r.AddAsync(It.IsAny<Todo>(), default));
    Assert.Equal("Valid Title", result.Title);
}
```

**Integration test catches the full flow:**
```csharp
[Fact]
public async Task EndpointRejects EmptyTitle()
{
    var client = /* test client */;
    
    var response = await client.PostAsJsonAsync("/todos", new { title = "" });
    
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    var error = await response.Content.ReadAsAsync<ErrorResponse>();
    Assert.Contains("cannot be empty", error.Message);
}
```

## Implementation Checklist

- [ ] Remove validation from `BackendApi.Application/UseCases/CreateTodoUseCase.cs`
- [ ] (Optional) Remove validation from `BackendApi.Application/UseCases/UpdateTodoUseCase.cs` if it exists
- [ ] Verify domain constructor still validates (no changes needed)
- [ ] Verify endpoint handles `ArgumentException` → HTTP 400
- [ ] Update use case tests to focus on orchestration, not validation
- [ ] Ensure integration tests verify invalid title is rejected at HTTP layer
- [ ] Run all tests; verify nothing breaks

## Future: Validation Abstraction

As validation rules grow more complex, consider extracting to a validation object:

```csharp
public record TodoTitle
{
    public string Value { get; init; }

    public TodoTitle(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Title cannot be empty or null");
        if (value.Length > 255)
            throw new ArgumentException("Title cannot exceed 255 characters");

        Value = value.Trim();
    }

    public static implicit operator string(TodoTitle title) => title.Value;
}

// Then in Todo:
public record Todo
{
    public required TodoTitle Title { get; init; }
    // ...
}
```

This keeps validation rules bundled with the concept they govern, improving locality.

## Related Deepening Opportunities

- **Deepening 3:** Remove Identity Pass-Through Use Cases (related: push logic to domain, not use cases)
- **Deepening 2:** Hoist Metrics Logic to Use Case (shows use case done right: orchestration, not duplication)
