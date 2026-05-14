Status: ready-for-agent

# Create backend project skeleton and references

## Parent

.scratch/split-backend-layered-projects/PRD.md

## What to build

Create the backend layered project skeleton so the existing Todo backend can be split across enforced .NET project boundaries without changing runtime behavior. The API host should remain the composition root, the frontend should remain untouched, and the new project/reference shape should match the accepted ADR.

## Acceptance criteria

- [ ] Separate backend projects exist for domain, application, and infrastructure alongside the existing API host.
- [ ] Project references enforce the intended dependency direction: API depends on application and infrastructure, infrastructure depends on application and domain, application depends on domain, and domain has no backend project dependencies.
- [ ] Namespaces match project boundaries and keep the `BackendApi.*` naming prefix.
- [ ] The solution builds far enough for later slices to move code into the new projects.
- [ ] No Todo REST API behavior is intentionally changed.

## Blocked by

None - can start immediately

