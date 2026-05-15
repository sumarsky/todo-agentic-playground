Status: done

# Move HTTP contracts and mapping to API host

## Parent

.scratch/split-backend-layered-projects/PRD.md

## What to build

Make the API host own Todo HTTP request and response contract types, mapping, endpoint routing, error handling, and composition. API endpoints should call concrete application use cases directly while preserving the existing public Todo REST API behavior.

## Acceptance criteria

- [ ] HTTP request and response types live under API `Contracts`.
- [ ] HTTP mapping code lives in the API host.
- [ ] API endpoints inject and call concrete application use cases directly.
- [ ] The API host calls `AddApplication()` and `AddInfrastructure()` for layer registration.
- [ ] Existing Todo endpoint routes, status codes, response shapes, and error behavior are preserved.
- [ ] API integration tests pass or are updated to assert the unchanged REST contract.
- [ ] The React frontend does not require changes.

## Blocked by

- .scratch/split-backend-layered-projects/issues/02-move-repository-port-and-application-use-cases.md
- .scratch/split-backend-layered-projects/issues/03-move-infrastructure-adapter-behind-addinfrastructure.md

