## 02 - Replace FilterBar with unified Toolbar

**Status:** ready-for-agent

### Description

Replace `FilterBar.jsx` with a new `Toolbar.jsx` component that contains: search input with `Search` icon, "Add todo" button with `Plus` icon, filter toggle with `Filter` icon, and a select-all checkbox. All in a single row.

### Acceptance Criteria

- Toolbar renders as a single horizontal row.
- Search input filters todos as the user types.
- Add button triggers `isAdding` state in TodoList.
- Filter toggle shows/hides completed todos.
- Select-all checkbox toggles selection of all visible todos.

### Testing

- Unit tests for each toolbar control's behavior.
- Integration test: toolbar interactions affect todo list state.
