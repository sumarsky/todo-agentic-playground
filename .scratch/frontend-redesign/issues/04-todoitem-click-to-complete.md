## 04 - TodoItem: click to complete, gray out completed

**Status:** ready-for-agent

### Description

Remove the completion checkbox from `TodoItem.jsx`. Make the todo text clickable to toggle completion. When `todo.completed` is true, apply grayed-out styling: `opacity: 0.6`, `text-decoration: line-through`, muted background.

### Acceptance Criteria

- Clicking todo text toggles completion status.
- Completed todos are visually distinct (grayed, strikethrough).
- Selection checkbox for bulk delete remains functional.
- Edit and delete buttons still work.

### Testing

- Unit tests: click toggles completion, completed styling applied.
- Integration test: complete/uncomplete cycle updates API and UI.
