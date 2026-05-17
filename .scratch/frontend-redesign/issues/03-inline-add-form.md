## 03 - Inline add form

**Status:** ready-for-agent

### Description

Remove the persistent `TodoForm` component. Instead, `TodoList` manages an `isAdding` state. When true, render a new `InlineAddForm` as the first `<li>` in the list. The form auto-focuses its input. Pressing Enter calls `addTodo()` and exits add mode. Pressing Escape exits without saving.

### Acceptance Criteria

- Clicking "Add todo" in the toolbar inserts a form as the first list item.
- Input is auto-focused when the form appears.
- Enter submits the todo and closes the form.
- Escape closes the form without submitting.
- Empty input does not submit.

### Testing

- Unit tests: Enter saves, Escape cancels, empty input blocked.
- Integration test: full add flow from button click to API call.
