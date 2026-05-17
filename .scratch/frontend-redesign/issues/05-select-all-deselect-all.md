## 05 - Select all / deselect all

**Status:** done

### Description

Add `selectAll` and `deselectAll` actions to `TodoContext`. The Toolbar renders a select-all checkbox that: checks when all visible todos are selected, unchecks when none are selected, shows indeterminate state when some are selected. Clicking toggles between select-all and deselect-all.

### Acceptance Criteria

- Select-all checkbox reflects current selection state accurately.
- Checking selects all visible todos.
- Unchecking deselects all todos.
- Indeterminate state shows when selection is partial.

### Testing

- Unit tests: selectAll/deselectAll context actions work correctly.
- Integration test: select-all checkbox state matches todo selection.
