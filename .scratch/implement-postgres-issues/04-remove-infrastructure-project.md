Status: done

# 04-remove-infrastructure-project

## Parent

[.scratch/implement-postgres-PRD.md](../implement-postgres-PRD.md)

## What to build

Remove the `BackendApi.Infrastructure` project entirely now that PostgreSQL storage is fully wired up. Delete the project directory, remove it from `Backend.slnx`, and remove the project reference from `BackendApi.Tests`. Update or remove any tests that directly reference `InMemoryTodoRepository` or `AddInfrastructure()`. The `Composition/LayerRegistrationTests` should be updated to verify that `ITodoRepository` resolves to `PostgresTodoRepository` (or the Postgres-backed implementation) after `AddPostgresStorage()` is called.

## Acceptance criteria

- [ ] `BackendApi.Infrastructure` directory deleted
- [ ] `BackendApi.Infrastructure` removed from `Backend.slnx`
- [ ] `BackendApi.Tests` no longer references `BackendApi.Infrastructure`
- [ ] No remaining references to `InMemoryTodoRepository` or `AddInfrastructure()` in codebase
- [ ] `Composition/LayerRegistrationTests` verifies Postgres-backed `ITodoRepository` resolution
- [ ] Solution builds successfully without the Infrastructure project
- [ ] All tests pass

## Blocked by

- [#03-postgres-write-operations](03-postgres-write-operations.md)
