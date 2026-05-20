# Refactor TodoProvider to Compose Three Hooks

**Status:** done

---

## What to build

Refactor `frontend/src/context/TodoContext.jsx` to compose the three independent hooks (`useTodoApi`, `useTodoFilters`, `useTodoSelection`) into a single provider that maintains the same stable public API.

The provider should:
1. Call each hook to get its state and methods
2. Add a `useEffect` that auto-loads todos on first render
3. Expose a unified context value with all three concerns (todos, filters, selection, and all methods)
4. Continue exporting `useTodoContext()` unchanged so existing components need no changes

The context value shape remains exactly the same as before, ensuring backward compatibility with all consuming components.

---

## Acceptance criteria

- [ ] TodoProvider refactored to use useTodoApi, useTodoFilters, useTodoSelection
- [ ] Auto-load on mount: useEffect calls listTodos() once on first render
- [ ] Context value shape unchanged: todos, loading, error, filters, selectedIds, and all methods
- [ ] useTodoContext() export unchanged—existing components work without modification
- [ ] Integration tests verify auto-load on mount, hook orchestration, context value structure
- [ ] All existing component tests continue to pass
- [ ] E2E tests continue to pass

## Blocked by

- [02-create-useTodoFilters-hook.md](02-create-useTodoFilters-hook.md)
- [03-create-useTodoSelection-hook.md](03-create-useTodoSelection-hook.md)
- [04-create-useTodoApi-hook.md](04-create-useTodoApi-hook.md)

---

## Parent

Source: `.scratch/extract-frontend-context/PRD.md`
