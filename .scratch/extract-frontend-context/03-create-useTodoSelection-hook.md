# Create useTodoSelection Hook

**Status:** ready-for-agent

---

## What to build

Create a pure state hook at `frontend/src/hooks/useTodoSelection.js` that manages selection state (`selectedIds` array) with sticky semantics. Deleted todos leave their IDs in `selectedIds`; components derive the active selected set by filtering `selectedIds` against the current todos list.

This hook uses only `useState` and internal logic—no `useContext` or dependencies on todo state. It exposes `selectedIds` state and methods `selectAll(count)`, `deselectAll()`, `selectTodo(id)`, `deselectTodo(id)`.

---

## Acceptance criteria

- [ ] Hook created at `frontend/src/hooks/useTodoSelection.js`
- [ ] Exposes `selectedIds` array and selection methods
- [ ] `selectAll(count)` accepts the number of todos to select
- [ ] Selection state is sticky (deleted todo IDs persist in selectedIds)
- [ ] No auto-cleanup when todos are deleted (caller responsibility)
- [ ] Unit tests verify selection mutations and stickiness
- [ ] Hook works in isolation (renderHook, no provider needed)

## Blocked by

None - can start immediately

---

## Parent

Source: `.scratch/extract-frontend-context/PRD.md`
