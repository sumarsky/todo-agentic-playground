# Issue: Backend - In-memory repository implementation

## What to build

Implement `InMemoryTodoRepository` adapter that stores todos in memory (`List<Todo>`). This completes the infrastructure layer for persistence.

**End-to-end**: Implement repository with all CRUD operations (Add, GetById, GetAll, Update, Delete, DeleteByIds). Write unit tests verifying each operation. Verify the repository is thread-safe for concurrent operations.

## Acceptance criteria

- [ ] `InMemoryTodoRepository` implements `ITodoRepository`
- [ ] All CRUD operations working (Add, GetById, GetAll, Update, Delete, DeleteByIds)
- [ ] Auto-incremented ID generation (1, 2, 3, ...)
- [ ] Repository unit tests pass
- [ ] Thread-safe for concurrent writes (at least basic locking)

## Blocked by

#003-backend-domain
