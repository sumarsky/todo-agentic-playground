# Issue: Frontend - Todo form & list components

## What to build

Implement React components for displaying and creating todos: `TodoForm`, `TodoList`, `TodoItem`. This is the core UI for basic CRUD operations.

**End-to-end**: 

1. **TodoForm**: Input field for title, submit button, calls `addTodo()` on submit, clears input, validates (no empty titles)
2. **TodoList**: Renders list of todos from context, handles empty state
3. **TodoItem**: Displays todo title, completion checkbox (toggle), delete button, edit/update action

All components wired to TodoContext. Component tests verify rendering, user interactions, and context calls.

## Acceptance criteria

- [ ] `TodoForm` component renders input, submit button; validates non-empty title
- [ ] `TodoForm` calls `context.addTodo()` on submit, clears input
- [ ] `TodoList` renders todos from context, shows empty state when no todos
- [ ] `TodoItem` renders todo title, completion checkbox, delete button
- [ ] Clicking checkbox calls `context.updateTodo()` to toggle completion
- [ ] Clicking delete button calls `context.deleteTodo()`
- [ ] Component tests pass (render, interactions, context calls mocked)

## Blocked by

#007-frontend-context
