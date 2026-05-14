Status: ready-for-agent

# Update tests and docs for split architecture

## Parent

.scratch/split-backend-layered-projects/PRD.md

## What to build

Update backend tests and documentation so they describe the new layered backend project split accurately. Keep one backend test project with folders per layer, preserve narrow layer tests, remove obsolete facade coverage, and keep the API contract focused on REST behavior and dependency rules.

## Acceptance criteria

- [ ] The backend test project references the backend layer projects needed for narrow layer tests.
- [ ] Obsolete application facade tests are removed or replaced with use-case/API coverage.
- [ ] Domain, application, infrastructure, API integration, and docs-contract tests pass.
- [ ] API contract documentation no longer claims the API depends on an application facade/service.
- [ ] API contract documentation stays focused on REST behavior and dependency rules rather than project inventory.
- [ ] ADR and context docs remain consistent with the implemented split.
- [ ] Backend test command passes.

## Blocked by

- .scratch/split-backend-layered-projects/issues/02-move-repository-port-and-application-use-cases.md
- .scratch/split-backend-layered-projects/issues/03-move-infrastructure-adapter-behind-addinfrastructure.md
- .scratch/split-backend-layered-projects/issues/04-move-http-contracts-and-mapping-to-api-host.md
