# PRD: Todo List Application

## Problem Statement

Users need a simple, responsive todo list application to manage tasks. Current solutions are either too complex (overkill for basic CRUD) or lack a clear separation between frontend and backend.

## Solution

Build a full-stack todo application with:
- **React frontend** for responsive UI with Tailwind CSS
- **.NET 10 backend** using Clean Architecture for testability and maintainability
- **In-memory storage** for simplicity (no database)
- **REST API** for frontend-backend communication
- **Comprehensive test coverage** on both frontend and backend

## User Stories

1. As a user, I want to create a new todo, so that I can track a new task
2. As a user, I want to view all my todos, so that I can see what needs to be done
3. As a user, I want to mark a todo as completed, so that I can track my progress
4. As a user, I want to update a todo's title, so that I can correct or refine the task
5. As a user, I want to delete a single todo, so that I can remove irrelevant tasks
6. As a user, I want to delete multiple todos at once, so that I can clean up completed items quickly
7. As a user, I want to filter todos by completion status, so that I can focus on pending or completed tasks
8. As a user, I want to search todos by title, so that I can find specific tasks
9. As a user, I want the app to work in my browser, so that I can access it without installation
10. As a user, I want API response errors to be clear, so that I understand what went wrong
11. As a developer, I want clean separation between business logic and infrastructure, so that I can test core logic in isolation
12. As a developer, I want unit tests on domain logic, so that I can refactor with confidence
13. As a developer, I want integration tests on use cases, so that I can verify orchestration works
14. As a developer, I want API contract tests, so that I can catch frontend-backend mismatches
15. As a developer, I want component tests on React, so that I can verify UI behavior

## Implementation Decisions

### Backend Architecture: Clean Architecture (Hexagonal)

The backend is organized in three layers:

**Domain Layer (Core Business Logic)**
- `Todo` entity with fields: id, title, completed, createdAt
- `ITodoRepository` port: interface for todo persistence
- Domain entities have no dependencies on frameworks or external libraries

**Application Layer (Use Cases)**
- `CreateTodoUseCase` — validates input, calls repository to persist
- `UpdateTodoUseCase` — updates todo title or completion status
- `DeleteTodoUseCase` — deletes a single todo
- `BulkDeleteTodoUseCase` — deletes multiple todos by id list
- `ListTodosUseCase` — fetches all todos, applies filtering (by completion, by title search)
- `TodoApplicationService` — orchestrates use cases, manages business flow

**Infrastructure Layer (Adapters)**
- `InMemoryTodoRepository` — implements `ITodoRepository`, stores todos in memory (`List<Todo>`)
- `TodoEndpoints` — ASP.NET Core Minimal API routes, request mapping, response formatting
- Global error handler — returns standard HTTP status codes (400, 404, 500) with JSON error details
- CORS middleware — allows requests from `http://localhost:3000` (React dev server)

**API Contract**
- `GET /todos?completed=true&search=text` — list todos with optional filtering
- `POST /todos` with body `{ "title": "string" }` — create todo
- `PUT /todos/{id}` with body `{ "title": "string", "completed": boolean }` — update todo
- `DELETE /todos/{id}` — delete single todo
- `DELETE /todos` with body `{ "ids": [1, 2, 3] }` — bulk delete todos

### Frontend State Management

**TodoContext** (React Context API)
- Holds list of todos, loading state, errors
- Provides async actions: `addTodo`, `updateTodo`, `deleteTodo`, `bulkDeleteTodos`, `listTodos`
- Fetches from backend API (`http://localhost:5000`)
- No external state library (xstate/Redux) — keeps implementation simple

**Component Hierarchy**
- `App` — providers setup
- `TodoList` — renders todo items and filter bar
- `TodoItem` — individual todo display, edit, delete buttons
- `TodoForm` — add new todo input
- `FilterBar` — completed filter toggle, search input
- `BulkDeleteButton` — checkbox selection, bulk delete action

**Styling**
- Tailwind CSS utility classes, no custom CSS framework

### ID Generation

Backend auto-generates sequential IDs (1, 2, 3, ...) for simplicity. IDs returned in API responses.

### Error Handling

**Backend**
- 400 Bad Request — validation failure (e.g., empty title)
- 404 Not Found — todo id doesn't exist
- 500 Internal Server Error — unexpected server error
- All errors return JSON: `{ "error": "string", "code": "string" }`

**Frontend**
- Catch fetch errors, display user-friendly messages
- Retry logic for transient failures (optional for MVP)

## Testing Decisions

### What Makes a Good Test

- **Unit tests**: Test a single function or class in isolation, mock external dependencies
- **Integration tests**: Test multiple components working together (e.g., use case + repository)
- **API contract tests**: Verify request/response shapes match between frontend and backend
- **Component tests**: Test React component rendering and user interactions, mock API calls
- Tests should verify external behavior, not implementation details (avoid over-mocking)

### Backend Testing

**Domain Tests**
- `TodoTests` — verify Todo entity invariants (id immutability, valid state transitions)

**Use Case Tests** (Unit)
- `CreateTodoUseCaseTests` — validates title, calls repository, returns created todo
- `UpdateTodoUseCaseTests` — validates id exists, updates title/completion
- `DeleteTodoUseCaseTests` — validates id exists, removes todo
- `BulkDeleteTodosUseCaseTests` — removes multiple todos, handles missing ids gracefully
- `ListTodosUseCaseTests` — filters by completion, searches by title substring

**Application Service Tests** (Integration)
- `TodoApplicationServiceTests` — orchestrates multiple use cases (e.g., create → list → update)

**Endpoint Tests** (Contract)
- `TodoEndpointTests` — verifies HTTP status codes, request validation, response shape

### Frontend Testing

**Context Tests** (Unit)
- `TodoContextTests` — state updates, async actions, error handling

**Component Tests**
- `TodoListTests` — renders todos, handles empty state
- `TodoItemTests` — render todo, edit/delete buttons, completion toggle
- `TodoFormTests` — form submission, validation (empty title)
- `FilterBarTests` — filter button click, search input change

**Integration Tests**
- `AppIntegrationTests` — full user flow (add → list → update → delete)

### Prior Art

Assume XUnit + Moq for .NET, Jest + React Testing Library for React.

## Out of Scope

- Database persistence (in-memory only)
- User authentication/authorization
- Undo/redo functionality
- Recurring todos or due dates
- Todo categories or tags
- Drag-and-drop reordering
- Offline support or sync
- Mobile app (web-only for now)
- Analytics or logging

## Further Notes

This is an MVP. The architecture (Clean/Hexagonal) is chosen for clarity and testability, not to optimize for scalability. If requirements change (e.g., add database, multi-user support), the domain and use cases remain stable—only infrastructure adapters change.

Frontend and backend can be deployed separately; they communicate via REST API only. No shared code or types (yet—could add OpenAPI schema generation in future).
