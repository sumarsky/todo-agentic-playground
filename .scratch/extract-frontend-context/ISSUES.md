# Published Issues Breakdown

**Source:** `PRD.md`  
**Published:** 2026-05-19  
**Status:** All issues ready-for-agent  

---

## Vertical Slice Issues

| # | Title | Type | User Stories | Blockers |
|---|-------|------|--------------|----------|
| 1 | Create apiClient Module | AFK | 1, 12, 13, 14 | None |
| 2 | Create useTodoFilters Hook | AFK | 2, 9, 15 | None |
| 3 | Create useTodoSelection Hook | AFK | 3, 10, 15 | None |
| 4 | Create useTodoApi Hook | AFK | 1, 7, 15 | Issue #1 |
| 5 | Refactor TodoProvider to Compose Three Hooks | AFK | 5, 8, 16 | Issues #2, #3, #4 |
| 6 | Verify E2E Tests Pass Unchanged | AFK | 17 | Issue #5 |

---

## Critical Path

```
Issue #1 (apiClient)
    ↓
Issue #4 (useTodoApi)
    ↓
Issue #5 (TodoProvider) ← Issue #2 (useTodoFilters)
    ↓                    ← Issue #3 (useTodoSelection)
Issue #6 (E2E verification)
```

**Independent tracks (can start in parallel):**
- **Track A:** Issues #1 → #4 → #5 → #6 (HTTP dependency chain)
- **Track B:** Issue #2 (useTodoFilters, no dependencies)
- **Track C:** Issue #3 (useTodoSelection, no dependencies)

All tracks converge at Issue #5 (TodoProvider composition).

---

## Issue Files

- `01-create-apiclient-module.md`
- `02-create-useTodoFilters-hook.md`
- `03-create-useTodoSelection-hook.md`
- `04-create-useTodoApi-hook.md`
- `05-refactor-TodoProvider-compose-hooks.md`
- `06-verify-e2e-tests-pass.md`

---

## Design Goals

✅ **Vertical slices**: Each issue is end-to-end, not horizontal layer-by-layer  
✅ **AFK automation**: All issues can be implemented without human decisions  
✅ **Testability**: Each concern is independently testable and composable  
✅ **Backward compatibility**: Public API remains stable; no component changes needed  
✅ **Parallelizable**: Independent concerns can be worked on simultaneously  
