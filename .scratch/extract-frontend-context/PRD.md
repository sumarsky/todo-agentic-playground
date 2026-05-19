# Extract Frontend Context into Independent Hooks

**Status:** ready-for-agent

---

## Problem Statement

The `TodoContext` violates the interface principle by mixing three unrelated concerns: HTTP communication, React state management, and UI state (filters, selections). This tight coupling makes the context difficult to test—unit tests require mocking both React hooks and global fetch simultaneously. It also prevents reuse: other components cannot import and use the HTTP client or filter logic independently.

The current architecture leads to shallow modules with high external complexity and low leverage.

---

## Solution

Extract the `TodoContext` into three independent, composable hooks:

- **`useTodoApi`** — encapsulates all HTTP communication and todo state (todos, loading, error)
- **`useTodoFilters`** — encapsulates filter state (completed, search)
- **`useTodoSelection`** — encapsulates selection state (selectedIds)

Create a supporting **`apiClient`** module that is a pure, stateless object exposing all HTTP operations.

The refactored `TodoProvider` composes these three hooks and exposes a unified `useTodoContext()` interface so existing components see no change.

---

## User Stories

1. As a frontend developer, I want the API client to be independently testable, so that I can verify HTTP behavior without mocking React hooks
2. As a frontend developer, I want filter state to be pure and testable, so that I can write fast unit tests for filter mutations
3. As a frontend developer, I want selection state to be pure and testable, so that I can write fast unit tests for selection logic
4. As a frontend developer, I want the three state concerns to be independently reusable, so that I can import the API client or hooks in other components without pulling in the full context
5. As a frontend developer, I want the context's public API to remain stable, so that I don't need to refactor components that use `useTodoContext()`
6. As a frontend developer, I want the API client to support dependency injection, so that I can inject a mock client in tests without mocking global fetch
7. As a frontend developer, I want to inject an alternative `apiClient` into `useTodoApi`, so that I can test hook behavior in isolation
8. As a frontend developer, I want the context to auto-load todos on app boot, so that the UI displays data without explicit component orchestration
9. As a frontend developer, I want filter changes to update state but not auto-trigger refetches, so that components have explicit control over when to call `listTodos()`
10. As a frontend developer, I want selection state to remain when todos are deleted, so that UI doesn't lose the semantic meaning of what the user selected
11. As a frontend developer, I want only the API hook to carry error state, so that pure state concerns (filters, selection) don't carry unused error properties
12. As a frontend developer, I want the apiClient to live in a clear location (`frontend/src/api/`), so that the HTTP boundary is obvious in the codebase
13. As a frontend developer, I want the HTTP client to support environment-based configuration, so that I can use `VITE_API_URL` to override the default backend URL
14. As a frontend developer, I want comprehensive tests for the API client, so that HTTP behavior is verified independently
15. As a frontend developer, I want comprehensive tests for each hook, so that state logic is verified in isolation
16. As a frontend developer, I want integration tests for the context provider, so that hook orchestration and auto-load behavior are verified
17. As a frontend developer, I want e2e tests to pass without modification, so that component behavior is unchanged

---

## Implementation Decisions

### 1. Modules to Create/Modify

**New modules (deep modules with stable interfaces):**

- **`apiClient`** — A stateless object providing HTTP methods: `listTodos(filters)`, `createTodo(title)`, `updateTodoTitle(id, title)`, `toggleTodo(id)`, `deleteTodo(id)`, `bulkDeleteTodos(ids)`. Handles fetch, error handling, URL construction, and response parsing. Lives in `frontend/src/api/apiClient.js`.

- **`useTodoApi`** — A React hook accepting an optional injected `apiClient` (defaults to the real one). Manages `todos`, `loading`, and `error` state. Exposes methods: `listTodo(filters)`, `addTodo(title)`, `updateTodo(id, updates)`, `deleteTodo(id)`, `bulkDeleteTodos(ids)`. Lives in `frontend/src/hooks/useTodoApi.js`.

- **`useTodoFilters`** — A React hook managing filter state: `filters = { completed, search }`. Exposes `setCompletedFilter(completed)` and `setSearchFilter(search)`. Pure state, no HTTP. Lives in `frontend/src/hooks/useTodoFilters.js`.

- **`useTodoSelection`** — A React hook managing selection state: `selectedIds` (array of IDs). Exposes `selectAll()`, `deselectAll()`, `selectTodo(id)`, `deselectTodo(id)`. Pure state, sticky (orphaned IDs remain when todos are deleted). Lives in `frontend/src/hooks/useTodoSelection.js`.

**Modified modules:**

- **`TodoProvider`** — Refactored to compose the three hooks. Maintains a `useEffect` that auto-loads todos on first render. Continues to export `useTodoContext()` as the stable public interface. No changes to the context value shape visible to consumers.

### 2. Hook Design & Independence

Each hook is pure (only `useState` and internal logic; no `useContext` or cross-hook dependencies). The provider wires them together and orchestrates side effects (e.g., auto-load on mount).

**Dependency injection for testability:**
- `useTodoApi` accepts an optional `apiClient` parameter. Tests can inject a mock to verify hook state mutations without mocking global fetch.

**Filter semantics:**
- Filter changes update state but do not trigger auto-refetch. Components explicitly call `listTodos(filters)` after changing filters. This keeps filter hook independent and makes component intent explicit.

**Selection semantics:**
- Selection state is sticky: deleted todos leave their IDs in `selectedIds`. Components derive `activeSelectedIds = selectedIds.filter(id => todos.some(t => t.id === id))` when rendering. This prevents cross-hook coupling (selection hook doesn't need to know about todos).

**Error state:**
- Only `useTodoApi` carries error state. Filters and selection are deterministic state transformations; they never fail.

### 3. API Client Design

**Environment configuration:**
```
const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';
```

Allows overriding the backend URL via `.env` without code changes.

**Stateless object (not a class):**
The apiClient is a plain object of methods, not a class instance. Simpler, easier to mock, no constructor complexity.

**Fetch error handling:**
Each method checks `response.ok` and throws with `Error(`HTTP ${status}`)`  or parsed error message from the response body.

### 4. Provider Orchestration

**Auto-load on mount:**
```javascript
useEffect(() => {
  listTodos();
}, []);
```

The context provider auto-loads todos on first render. This is orchestration logic, not hook logic—it belongs in the provider.

**Context value shape (unchanged):**
```javascript
{
  todos,
  loading,
  error,
  filters,
  selectedIds,
  listTodos,
  setCompletedFilter,
  setSearchFilter,
  addTodo,
  updateTodo,
  deleteTodo,
  bulkDeleteTodos,
  selectAll,
  deselectAll,
  selectTodo,
  deselectTodo,
}
```

All existing consumers of `useTodoContext()` see the same interface. No component changes required.

### 5. Test Strategy

- **API client unit tests** — Mock global fetch. Verify HTTP method behavior (correct URL, headers, error handling).
- **Hook unit tests** — Inject mock apiClient into `useTodoApi`. Verify state mutations, loading/error transitions, list/add/update/delete operations. Test `useTodoFilters` and `useTodoSelection` in isolation (pure state).
- **Provider integration tests** — Verify auto-load on mount, hook orchestration, context value shape.
- **E2E tests** — Run existing e2e tests unchanged. Verify no regressions in user workflows.

---

## Testing Decisions

### Good Tests

A good test verifies **external behavior** (what the hook or module does), not implementation details (how it does it). A good test is:
- **Isolated** — tests one module/concern in isolation
- **Deterministic** — no timing dependencies or global state
- **Fast** — runs in milliseconds
- **Readable** — test name and assertions are clear about what behavior is verified

### Which Modules Will Be Tested

All six modules will have tests:
1. `apiClient` — unit tests
2. `useTodoApi` — unit tests with mock injection
3. `useTodoFilters` — unit tests
4. `useTodoSelection` — unit tests
5. `TodoProvider` — integration tests
6. Components using `useTodoContext()` — existing tests unchanged

### Prior Art

The codebase already uses Vitest for component and context tests. Existing context tests should serve as the template for TodoProvider integration tests.

---

## Out of Scope

- **Moving filter or selection UI logic to components** — The provider continues to manage all three concerns; components use `useTodoContext()` as before.
- **Metrics or logging observability** — The refactor does not add instrumentation.
- **Retry logic or circuit breaker** — The API client is simple fetch-based for now.
- **API configuration beyond environment variables** — `VITE_API_URL` is the only lever.
- **Cascading deletes or side effects** — Selection remains sticky; no auto-cleanup of orphaned IDs.

---

## Further Notes

### ADR

This refactor is documented in **ADR-0002: Extract Frontend Context into Independent Hooks**. It records the trade-offs (more files for better testability) and the decision to keep the public API stable.

### Incremental Integration

After implementation, all three hooks can be gradually exported for use in other components if the need arises. For now, they are internal to the context and only consumed by the provider.

### Swappability

The main benefit of this refactor is that `apiClient` can be replaced (e.g., axios instead of fetch, retry logic, request interceptors) by editing one file. The hooks will continue to work unchanged.
