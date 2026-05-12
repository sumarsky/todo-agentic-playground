# Issue: Frontend - TodoContext API client

## What to build

Implement TodoContext actions (async functions) that fetch from the backend API (`http://localhost:5000`). This connects the frontend state to the backend.

**End-to-end**: Add actions to TodoContext: `listTodos()`, `addTodo(title)`, `updateTodo(id, updates)`, `deleteTodo(id)`, `bulkDeleteTodos(ids)`. Each action updates context state, handles loading/error states, and makes HTTP requests to backend. Write context unit tests mocking API calls.

## Acceptance criteria

- [ ] `listTodos()` fetches GET /todos and updates state
- [ ] `addTodo(title)` POSTs to /todos, updates state
- [ ] `updateTodo(id, updates)` PUTs to /todos/{id}, updates state
- [ ] `deleteTodo(id)` DELETEs /todos/{id}, updates state
- [ ] `bulkDeleteTodos(ids)` DELETEs /todos with body, updates state
- [ ] Loading state managed (set before request, clear after)
- [ ] Error state managed (set on failure, clear on success)
- [ ] Context unit tests pass (state updates, error handling, mocked API calls)

## Blocked by

#006-frontend-scaffold
