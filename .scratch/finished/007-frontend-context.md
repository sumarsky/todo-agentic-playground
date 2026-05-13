# Issue: Frontend - TodoContext API client

## What to build

Implement TodoContext actions (async functions) that fetch from the backend API (`http://localhost:5000`). This connects the frontend state to the backend.

**End-to-end**: Add actions to TodoContext: `listTodos()`, `addTodo(title)`, `updateTodo(id, updates)`, `deleteTodo(id)`, `bulkDeleteTodos(ids)`. Each action updates context state, handles loading/error states, and makes HTTP requests to backend. Write context unit tests mocking API calls.

## Acceptance criteria

- [x] `listTodos()` fetches GET /todos and updates state
- [x] `addTodo(title)` POSTs to /todos, updates state
- [x] `updateTodo(id, updates)` PUTs to /todos/{id}, updates state
- [x] `deleteTodo(id)` DELETEs /todos/{id}, updates state
- [x] `bulkDeleteTodos(ids)` DELETEs /todos with body, updates state
- [x] Loading state managed (set before request, clear after)
- [x] Error state managed (set on failure, clear on success)
- [x] Context unit tests pass (state updates, error handling, mocked API calls)

## Blocked by

#006-frontend-scaffold
