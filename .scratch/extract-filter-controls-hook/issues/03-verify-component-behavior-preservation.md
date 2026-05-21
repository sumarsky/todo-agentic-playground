# Verify Component Behavior Preservation

Status: ready-for-agent

## Parent

.scratch/extract-filter-controls-hook/PRD.md

## What to build

Verify that the filter controls extraction preserved the existing todo UI behavior. The goal is not to add hook-mocking component tests for their own sake, but to ensure the toolbar and filter bar still support the same user-visible workflows after the shared hook is introduced.

Verification should cover filtering by search text, toggling the completed filter, toolbar add behavior, toolbar select-all behavior, and existing labels/icons/accessibility expectations where the current test suite already checks them or where low-cost verification is available.

## Acceptance criteria

- [ ] Existing frontend tests pass after the extraction.
- [ ] Existing lint checks pass after the extraction.
- [ ] Search filtering still works through the toolbar and filter bar entry points available in the app.
- [ ] Completed filtering still works through the toolbar and filter bar entry points available in the app.
- [ ] Toolbar add todo behavior still works.
- [ ] Toolbar select-all and indeterminate behavior still work.
- [ ] No new component tests are added solely to mock the filter controls hook unless an existing test must be adjusted to preserve user-visible behavior coverage.

## Blocked by

- .scratch/extract-filter-controls-hook/issues/02-extract-search-filter-control.md
