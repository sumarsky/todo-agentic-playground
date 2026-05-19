# Extract Frontend Context into Independent Hooks

We split `TodoContext` into three independent hooks—`useTodoApi`, `useTodoFilters`, and `useTodoSelection`—to isolate HTTP concerns from state management and improve testability. Each hook is pure; the context orchestrates them. This trades simplicity in the provider for locality and reusability in each hook.

## Context

The original `TodoContext` mixed three concerns:
1. HTTP calls to backend endpoints (fetch, error handling, URL construction)
2. State management (todos, loading, error state)
3. UI state (filters, selections)

This tight coupling made testing difficult—unit tests of context logic required mocking React hooks and fetch simultaneously. It also prevented reuse: other components couldn't use the HTTP client or filters logic independently.

## Decision

We introduced three independent, composable hooks:
- **`useTodoApi(apiClient?)`** — manages todos and loading/error state; calls HTTP endpoints via injected `apiClient`
- **`useTodoFilters()`** — manages filter state (completed, search); no HTTP or side effects
- **`useTodoSelection()`** — manages selected todo IDs; deterministic state only

The context provider composes these hooks and exposes a unified `useTodoContext()` interface. Components see no change.

## Consequences

**Positive:**
- **Testable in isolation** — `useTodoApi` can be tested with a mock apiClient; no React hook mocking needed
- **Reusable** — other contexts or components can import and use `useTodoApi` or `useTodoFilters` directly
- **Clear separation** — HTTP logic lives in `api/apiClient.js`, state logic in hooks
- **Swappable** — to change HTTP library or add retry logic, edit one file

**Trade-offs:**
- **More files** — three hooks instead of one context
- **Context provider gets simpler but longer** — must wire hooks and manage their interaction (e.g., auto-load on mount)
- **Orchestration logic moves to provider** — e.g., context `useEffect` triggers `listTodos()` on app boot instead of hook

## Related Decisions

- **ADR-0001** (backend layering) — mirrors this pattern on the backend: separation of concerns, easier testing
