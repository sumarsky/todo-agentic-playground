# Create apiClient Module

**Status:** done

---

## What to build

Create a stateless HTTP client module at `frontend/src/api/apiClient.js` that encapsulates all HTTP communication with the backend API. The client exposes methods for each todo operation and metrics/logs operations, handles environment-based URL configuration, and manages fetch error handling.

The module should be a plain object of functions (not a class), accept `VITE_API_URL` environment variable to override the default backend URL, and throw descriptive errors on HTTP failures.

---

## Acceptance criteria

- [x] Module created at `frontend/src/api/apiClient.js`
- [x] All six todo methods implemented: `listTodos`, `createTodo`, `updateTodoTitle`, `toggleTodo`, `deleteTodo`, `bulkDeleteTodos`
- [x] All response status codes are checked; HTTP errors throw with `Error('HTTP ${status}')` or parsed message
- [x] Environment variable `VITE_API_URL` overrides default `http://localhost:5000`
- [x] Unit tests for apiClient cover all methods, error cases, and URL construction
- [x] Module can be imported and mocked in other files without side effects

## Blocked by

None - can start immediately

---

## Parent

Source: `.scratch/extract-frontend-context/PRD.md`

---

## Implementation Notes

- 11 unit tests via Vitest, all passing
- `checkResponse()` helper centralizes HTTP error handling
- Plain object export, zero side effects, fully mockable
