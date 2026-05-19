# Create useTodoFilters Hook

**Status:** ready-for-agent

---

## What to build

Create a pure state hook at `frontend/src/hooks/useTodoFilters.js` that manages filter state (completed, search) independently of HTTP or other concerns. The hook should not trigger refetches automatically; components explicitly call `listTodos()` after changing filters.

This hook uses only `useState` and internal logic—no `useContext` or cross-hook dependencies. It exposes `filters` state and methods `setCompletedFilter()` and `setSearchFilter()`.

---

## Acceptance criteria

- [ ] Hook created at `frontend/src/hooks/useTodoFilters.js`
- [ ] Exposes `filters` object with `completed` and `search` fields
- [ ] Exposes `setCompletedFilter(completed)` and `setSearchFilter(search)` methods
- [ ] Filter changes update state but do not trigger HTTP calls
- [ ] Unit tests verify filter state mutations and independence from HTTP
- [ ] Hook works in isolation (renderHook, no provider needed)

## Blocked by

None - can start immediately

---

## Parent

Source: `.scratch/extract-frontend-context/PRD.md`
