Status: ready-for-agent

# Remove Duplicate Pass-Through Use Case Tests

## Parent

.scratch/remove-identity-passthrough-tests/PRD.md

## What to build

Remove the duplicate application-level tests for identity pass-through todo use
cases while preserving the documented API-to-application boundary. The list,
delete, and bulk delete use cases should remain in place, endpoints should
continue to call use cases, and dependency injection registration should remain
unchanged.

The completed change should leave behavior covered by the deeper modules that
own it: API integration tests for HTTP behavior, repository tests for
persistence and filtering/search semantics, and composition tests for use case
registration.

## Acceptance criteria

- [ ] The application tests for the list, delete, and bulk delete pass-through
      use cases are removed.
- [ ] The list, delete, and bulk delete use cases remain in the application
      layer.
- [ ] API endpoints continue to inject and call application use cases rather
      than repository ports directly.
- [ ] Dependency injection registrations for the remaining use cases are
      unchanged.
- [ ] No replacement smoke tests are added for the deleted pass-through tests.
- [ ] Existing API integration tests still cover delete, bulk delete, list,
      completed filtering, and search behavior.
- [ ] Existing repository tests still cover persistence, filtering, and search
      behavior.
- [ ] Existing composition tests still cover use case dependency injection
      registration.
- [ ] `dotnet test` passes.

## Blocked by

None - can start immediately
