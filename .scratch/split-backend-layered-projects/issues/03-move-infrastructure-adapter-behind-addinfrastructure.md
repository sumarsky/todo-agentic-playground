Status: ready-for-agent

# Move infrastructure adapter behind AddInfrastructure

## Parent

.scratch/split-backend-layered-projects/PRD.md

## What to build

Move the in-memory Todo repository into the infrastructure project and wire it as the adapter for the application repository port. Normal app wiring should use an infrastructure dependency injection extension while infrastructure tests can still instantiate the concrete adapter directly.

## Acceptance criteria

- [ ] The in-memory repository lives in the infrastructure project.
- [ ] The in-memory repository implements the application repository port.
- [ ] Infrastructure dependency injection is exposed through `AddInfrastructure()`.
- [ ] The API composition root can register the repository implementation through the infrastructure extension.
- [ ] The concrete in-memory repository remains public for direct infrastructure tests.
- [ ] Infrastructure tests pass or are updated to target the new infrastructure boundary.

## Blocked by

- .scratch/split-backend-layered-projects/issues/01-create-backend-project-skeleton-and-references.md
- .scratch/split-backend-layered-projects/issues/02-move-repository-port-and-application-use-cases.md

