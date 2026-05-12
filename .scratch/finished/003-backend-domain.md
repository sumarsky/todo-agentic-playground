# Issue: Backend - Todo domain entity & repository interface

## What to build

Define the `Todo` entity and `ITodoRepository` port interface (dependency inversion). This establishes the core domain layer with no framework dependencies.

**End-to-end**: Create `Todo` value object with fields (id, title, completed, createdAt) and immutable id. Define `ITodoRepository` interface with CRUD operations. Write unit tests verifying entity invariants.

## Acceptance criteria

- [ ] `Todo` entity defined with id, title, completed, createdAt fields
- [ ] `ITodoRepository` interface defined with methods: Add, GetById, GetAll, Update, Delete, DeleteByIds
- [ ] Domain tests pass (entity immutability, state validation)

## Blocked by

#002-backend-minimal-api
