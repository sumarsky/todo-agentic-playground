Status: ready-for-agent

# PRD: Remove Identity Pass-Through Tests

## Problem Statement

The backend test suite includes application-level tests for use cases that only
forward calls to repository ports. Those use cases are still valuable as the
documented API-to-application boundary, but their pass-through tests duplicate
coverage that already exists in API integration tests, repository tests, and
composition tests.

This creates maintenance cost without improving confidence. A future agent
working on todo deletion, bulk deletion, listing, filtering, or search has to
update tests that mostly verify implementation plumbing rather than externally
observable behavior.

## Solution

Remove the application unit tests that exercise identity pass-through use cases
for listing todos, deleting one todo, and bulk deleting todos. Keep the use
cases themselves, keep endpoint injection through use cases, and keep dependency
injection registrations unchanged.

The resulting test suite should continue to prove behavior through the deeper
modules that own it:

- API integration tests prove HTTP behavior and contract behavior.
- Repository tests prove persistence, filtering, search, delete, and bulk delete
  behavior.
- Composition tests prove use case registration and dependency wiring.

## User Stories

1. As a backend maintainer, I want pass-through use case tests removed, so that
   the test suite focuses on behavior rather than forwarding.
2. As a backend maintainer, I want the API-to-application boundary preserved, so
   that the implementation remains aligned with the backend layering decision.
3. As a backend maintainer, I want todo delete behavior covered through API and
   repository tests, so that confidence comes from observable behavior and the
   module that owns persistence.
4. As a backend maintainer, I want bulk delete behavior covered through API and
   repository tests, so that duplicate application tests do not need to change
   when repository details change.
5. As a backend maintainer, I want todo list, filter, and search behavior covered
   through API and repository tests, so that filtering semantics are tested where
   they are implemented and exposed.
6. As a future implementation agent, I want a narrow cleanup scope, so that I do
   not accidentally change endpoint wiring, dependency registration, or use case
   structure.
7. As a future implementation agent, I want no replacement smoke tests for these
   pass-through use cases, so that low-value coupling is not reintroduced under
   another name.
8. As a reviewer, I want the test deletion justified by existing coverage, so
   that the change reads as deliberate test-suite maintenance rather than missing
   coverage.
9. As a reviewer, I want the backend tests to pass after the deletion, so that
   the cleanup does not hide a real behavioral regression.
10. As a developer changing repository behavior, I want repository tests to be
    the source of truth for persistence semantics, so that changes are localized.
11. As a developer changing HTTP behavior, I want API integration tests to be the
    source of truth for request and response behavior, so that application tests
    do not duplicate contract assertions.
12. As a developer changing dependency injection, I want composition tests to
    catch missing use case registrations, so that pass-through unit tests are not
    used as wiring tests.
13. As a project maintainer, I want the issue severity to reflect the real risk,
    so that this cleanup is treated as low-severity maintenance instead of
    critical architecture work.
14. As a project maintainer, I want no new architecture decision record for this
    change, so that documentation remains focused on hard-to-reverse decisions
    with genuine trade-offs.
15. As a project maintainer, I want the existing domain glossary left unchanged,
    so that implementation and testing policy does not pollute domain language.

## Implementation Decisions

- Preserve the backend layered architecture: API endpoints call application use
  cases, and repository ports remain behind the application boundary.
- Preserve one use case per API operation for the current todo API shape.
- Do not introduce a grouped todo application service as part of this cleanup.
- Do not inject repository ports directly into API endpoints.
- Do not delete the list, delete, or bulk delete use cases.
- Do not remove or change dependency injection registrations for these use
  cases.
- Delete the application unit tests whose only value is exercising identity
  forwarding for list, delete, and bulk delete use cases.
- Do not replace the deleted tests with smoke tests.
- Keep the implementation limited to test-suite cleanup unless running the tests
  exposes a genuine coverage or wiring gap.
- Treat this as low-severity maintenance, not critical architecture work.
- Do not create a new ADR for this change because it follows the existing
  backend layering decision.
- Do not update the domain glossary because no product or domain term is being
  introduced or clarified.

## Testing Decisions

- Good tests should verify externally meaningful behavior or the behavior owned
  by a deep module, not implementation forwarding between adjacent layers.
- API integration tests should remain the coverage source for HTTP todo behavior,
  including delete, bulk delete, list, completed filtering, and search.
- Repository tests should remain the coverage source for persistence behavior,
  including list, completed filtering, case-insensitive search, combined filters,
  delete, and bulk delete behavior.
- Composition tests should remain the coverage source for use case dependency
  injection registration.
- Application tests should remain valuable where use cases enforce business
  invariants or orchestration, such as title validation, not-found handling,
  update/toggle state transitions, and metrics calculation.
- After deleting the pass-through tests, run the backend test suite.
- Add tests only if the deletion reveals a real gap in API, repository, or
  composition coverage.

## Out of Scope

- Changing API endpoint signatures or routing.
- Injecting repositories directly into endpoints.
- Deleting use cases.
- Renaming use cases or changing their public methods.
- Changing dependency injection registration.
- Introducing a grouped application service.
- Rewriting API integration tests.
- Rewriting repository tests.
- Adding a testing policy document.
- Updating the domain glossary.
- Creating a new architecture decision record.

## Further Notes

This PRD came from the grill-with-docs session for the original deepening note.
The original proposal conflicted with the documented backend layering decision
because it would have bypassed application use cases from API endpoints. The
resolved version keeps the architecture intact and narrows the work to removing
duplicate pass-through tests.
