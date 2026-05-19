# Create apiClient Module

**Status:** ready-for-agent

---

## What to build

Create a stateless HTTP client module at `frontend/src/api/apiClient.js` that encapsulates all HTTP communication with the backend API. The client exposes methods for each todo operation and metrics/logs operations, handles environment-based URL configuration, and manages fetch error handling.

The module should be a plain object of functions (not a class), accept `VITE_API_URL` environment variable to override the default backend URL, and throw descriptive errors on HTTP failures.

---

## Acceptance criteria

- [ ] Module created at `frontend/src/api/apiClient.js`
- [ ] All six todo methods implemented: `listTodos`, `createTodo`, `updateTodoTitle`, `toggleTodo`, `deleteTodo`, `bulkDeleteTodos`
- [ ] All response status codes are checked; HTTP errors throw with `Error('HTTP ${status}')` or parsed message
- [ ] Environment variable `VITE_API_URL` overrides default `http://localhost:5000`
- [ ] Unit tests for apiClient cover all methods, error cases, and URL construction
- [ ] Module can be imported and mocked in other files without side effects

## Blocked by

None - can start immediately

---

## Parent

Source: `.scratch/extract-frontend-context/PRD.md`
