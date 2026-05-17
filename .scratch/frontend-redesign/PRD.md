## Problem Statement

The current todo app frontend has poor UX: todos don't load on startup, the double-checkbox pattern is confusing, there's no select-all, the add form and search bar are awkwardly placed side-by-side, the UI lacks icons and tooltips, and the dark purple theme is visually unappealing.

## Solution

Redesign the frontend with: todos loading on mount, single-click text toggle for completion with grayed-out styling, a select-all checkbox, an inline add form triggered by a button, a unified toolbar layout, lucide-react icons with tooltips, and a light green/olive gradient theme.

## User Stories

1. As a user, I want all my todos to appear when I open the app, so that I don't have to manually trigger a load.
2. As a user, I want to mark a todo complete by clicking its text, so that I don't need a separate checkbox.
3. As a user, I want completed todos to appear grayed out with strikethrough text, so that I can visually distinguish them from active todos.
4. As a user, I want a select-all checkbox so that I can quickly select every visible todo for bulk deletion.
5. As a user, I want to deselect all todos with a single click, so that I can undo a bulk selection.
6. As a user, I want an "Add todo" button that inserts a new form at the top of the list, so that adding todos feels contextual and doesn't clutter the UI.
7. As a user, I want to press Enter to save a new todo from the inline form, so that I can add todos quickly.
8. As a user, I want to press Escape to cancel the inline add form, so that I can back out without creating an empty todo.
9. As a user, I want a single toolbar row with search, add, and filter controls, so that the layout feels organized and intuitive.
10. As a user, I want icons on action buttons with tooltips on hover, so that the UI is scannable and self-explanatory.
11. As a user, I want a light theme with a green/olive gradient background, so that the app is pleasant to look at.

## Implementation Decisions

### Modules to modify

- **TodoContextProvider** — Add `useEffect` on mount to call `listTodos()`. Add `selectAll` and `deselectAll` actions to the context value.
- **Toolbar** (new) — Replaces `FilterBar`. Single-row layout containing: search input with `Search` icon, "Add todo" button with `Plus` icon, filter toggle with `Filter` icon, and select-all checkbox.
- **TodoList** — Accepts an `isAdding` state. When true, renders `InlineAddForm` as the first child of the list. Integrates select-all checkbox from context.
- **InlineAddForm** (new) — Renders an input field at the top of the list. On Enter, calls `addTodo()` and exits add mode. On Escape, exits add mode without saving.
- **TodoItem** — Remove the completion checkbox. The todo text/span becomes clickable to toggle completion. When `todo.completed` is true, apply grayed-out styling with strikethrough.
- **BulkDeleteButton** — Move into the toolbar row. Keep existing behavior.
- **Styling (App.css, index.css)** — Replace purple/dark theme with green/olive gradient. Remove `prefers-color-scheme` dark mode override. Add CSS classes for completed todo styling.

### Theme tokens

```css
:root {
  --bg-gradient-start: #f5f7f0;
  --bg-gradient-end: #e8f0dc;
  --accent: #6b8f3c;
  --accent-hover: #5a7a32;
  --text: #3a3a3a;
  --text-muted: #8a8a8a;
  --text-h: #1a1a1a;
  --border: #d4dcc8;
  --completed-bg: #f0f0f0;
  --completed-text: #999;
}
```

### Inline add form behavior

The `TodoList` component owns an `isAdding` boolean state. When true, the first `<li>` contains the `InlineAddForm`. The form auto-focuses its input on mount.

### Completed todo styling

Completed todos receive: `opacity: 0.6`, `text-decoration: line-through`, and a muted background. The entire row is visually de-emphasized.

### Icon library

`lucide-react` for all icons. Buttons use icon-only with `aria-label` and a CSS tooltip on hover.

### Select-all behavior

A checkbox in the toolbar. Checked when all visible todos are selected. Unchecked when none are selected. Indeterminate when some are selected. Toggles between select-all and deselect-all.

## Testing Decisions

All modules will be tested. Tests should verify external behavior, not implementation details.

- **TodoContextProvider** — Tests that `listTodos()` is called on mount, that `selectAll`/`deselectAll` update selection state correctly, and that existing CRUD operations still work. Prior art: `TodoContextProvider.test.jsx`, `TodoContext.test.jsx`.
- **Toolbar** — Tests that search input updates filter, add button triggers add mode, filter toggle works, select-all checkbox toggles selection. Prior art: no existing Toolbar tests (new component).
- **TodoList** — Tests that inline form appears when `isAdding` is true, that select-all checkbox renders and works, that todos render correctly. Prior art: `TodoComponents.test.jsx`.
- **InlineAddForm** — Tests that Enter calls `addTodo()` and exits, Escape exits without saving, empty input does not submit. Prior art: no existing tests (new component).
- **TodoItem** — Tests that clicking todo text toggles completion, that completed todos render with grayed styling, that edit and delete still work. Prior art: `TodoComponents.test.jsx`.
- **BulkDeleteButton** — Tests that button is disabled when nothing selected, that clicking deletes selected todos. Prior art: no existing tests (component exists but untested).
- **Integration** — Full app flow: load todos, add one, complete one, select all, bulk delete. Prior art: `App.integration.test.jsx`.

## Out of Scope

- Backend API changes — the existing `/todos` endpoints are sufficient.
- Drag-and-drop reordering of todos.
- Categories, tags, or due dates.
- Persistent theme preferences.
- Mobile-responsive redesign beyond the existing breakpoints.

## Further Notes

The existing Tailwind setup in `index.css` is not actively used by the app components (they use plain CSS in `App.css`). Consider whether to keep or remove Tailwind. The redesign will use plain CSS for consistency with the existing approach.
