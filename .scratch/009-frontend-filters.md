# Issue: Frontend - Filter & bulk delete

## What to build

Implement `FilterBar` and `BulkDeleteButton` components for filtering todos and bulk deleting. Extend UI with checkbox selection and filter controls.

**End-to-end**:

1. **FilterBar**: Toggle button for "Completed" filter, search input for title search; updates context on filter/search change
2. **BulkDeleteButton**: Checkboxes on TodoItem for selection, bulk delete button, calls `context.bulkDeleteTodos(selectedIds)` 
3. Wire filters to `listTodos()` API call (pass query params)

Component tests verify interactions, state updates, API calls.

## Acceptance criteria

- [ ] `FilterBar` component with "Completed" toggle and search input
- [ ] Toggling "Completed" filter updates context and refetches todos
- [ ] Search input updates context state and refetches todos with query param
- [ ] TodoItem checkbox allows selection
- [ ] `BulkDeleteButton` shows selected count
- [ ] Clicking bulk delete button calls `context.bulkDeleteTodos()` with selected IDs
- [ ] Component tests pass (filter actions, bulk delete, API calls)

## Blocked by

#007-frontend-context
#008-frontend-components
