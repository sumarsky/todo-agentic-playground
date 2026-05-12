# Issue: Backend - All use cases & REST endpoints (merged)

## What to build

Implement all use cases (`CreateTodoUseCase`, `UpdateTodoUseCase`, `DeleteTodoUseCase`, `BulkDeleteTodoUseCase`, `ListTodosUseCase`), orchestrator service, and REST endpoints. This is the complete application layer.

**End-to-end**:

1. **Use Cases**: Implement each use case validating input, calling repository, returning results
2. **ListTodosUseCase**: Support filtering by completion status (`?completed=true/false`) and searching by title substring (`?search=text`)
3. **Endpoints**: Wire all HTTP routes (GET/POST/PUT/DELETE /todos, DELETE /todos for bulk)
4. **TodoApplicationService**: Orchestrate use cases, manage business flow
5. **Tests**: Unit tests for each use case, integration tests for service, contract tests for endpoints

## Acceptance criteria

- [ ] `CreateTodoUseCase` with title validation; POST /todos endpoint working
- [ ] `ListTodosUseCase` with filtering (completed, search); GET /todos endpoint working
- [ ] `UpdateTodoUseCase` for title/completion updates; PUT /todos/{id} endpoint working
- [ ] `DeleteTodoUseCase` & `BulkDeleteTodoUseCase`; DELETE /todos/{id} and DELETE /todos endpoints working
- [ ] `TodoApplicationService` orchestrates all use cases
- [ ] All use case unit tests passing
- [ ] Application service integration tests passing
- [ ] API contract tests passing (status codes, error formats, response shape)
- [ ] Error handling: 400 (validation), 404 (not found), 500 (server error)

## Blocked by

#004-backend-repository
