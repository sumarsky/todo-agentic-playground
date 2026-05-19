# Create useTodoApi Hook

**Status:** done

---

## What to build

Create a React hook at `frontend/src/hooks/useTodoApi.js` that manages todo state (todos, loading, error) and orchestrates HTTP calls. The hook accepts an optional injected `apiClient` parameter (defaults to the real one) to enable test-time mock injection.

The hook exposes `todos`, `loading`, `error` state and methods: `listTodos(filters)`, `addTodo(title)`, `updateTodo(id, updates)`, `deleteTodo(id)`, `bulkDeleteTodos(ids)`.

This hook uses only `useState` and the injected apiClient—no `useContext` or cross-hook dependencies. Error handling, loading states, and HTTP orchestration belong here.

---

## Acceptance criteria

- [ ] Hook created at `frontend/src/hooks/useTodoApi.js`
- [ ] Accepts optional `apiClient` parameter for dependency injection
- [ ] Exposes `todos`, `loading`, `error` state
- [ ] All five methods implemented: `listTodos`, `addTodo`, `updateTodo`, `deleteTodo`, `bulkDeleteTodos`
- [ ] Loading state managed: set to true before request, false after
- [ ] Error state managed: set on failure, cleared on success
- [ ] Unit tests inject mock apiClient and verify state mutations without mocking global fetch
- [ ] Hook works in isolation (renderHook, no provider needed)

## Blocked by

- [01-create-apiclient-module.md](01-create-apiclient-module.md)

---

## Parent

Source: `.scratch/extract-frontend-context/PRD.md`
