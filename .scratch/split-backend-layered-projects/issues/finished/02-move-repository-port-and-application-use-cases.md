Status: ready-for-agent

# Move repository port and application use cases

## Parent

.scratch/split-backend-layered-projects/PRD.md

## What to build

Move the Todo application boundary into the application project. The repository port should live under application `Ports`, concrete use cases should remain the API-facing application operations, and the forwarding application facade should be removed.

## Acceptance criteria

- [ ] The repository port lives in the application layer under `Ports`.
- [ ] Domain code has no persistence port or infrastructure dependency.
- [ ] Todo use cases live in the application project and depend on the repository port and domain model.
- [ ] Use cases remain concrete classes without one-method interfaces.
- [ ] The forwarding application facade is removed or no longer used.
- [ ] Application dependency injection is exposed through `AddApplication()`.
- [ ] No clock or ID generator ports are introduced.
- [ ] Application use-case tests pass or are updated to target the new application boundary.

## Blocked by

- .scratch/split-backend-layered-projects/issues/01-create-backend-project-skeleton-and-references.md

