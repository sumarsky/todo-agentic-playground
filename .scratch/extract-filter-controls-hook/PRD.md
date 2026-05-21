# Extract Frontend Filter Controls Hook

Status: ready-for-agent

## Problem Statement

Users can filter todos by completion state and search by title, but the frontend currently duplicates the same filter command logic in more than one UI component. When a developer needs to change how filter commands compose the next filter payload and trigger todo loading, they must update multiple components consistently.

This duplication makes the frontend harder to maintain and harder to test. The behavior is not complex, but it is currently mixed into rendering code, so tests must go through component rendering to verify command composition.

## Solution

Extract the shared todo filter command handlers into a reusable frontend hook. The hook will expose the current filters plus command functions for toggling the completed filter and updating the search filter.

The extraction must preserve the existing architecture from ADR-0002: the existing filter hook owns filter state and stays side-effect free, while the context provider composes filter state with API loading. The new hook coordinates UI commands against the existing context; it must not introduce a second source of filter state.

The user-facing behavior must remain unchanged. Search stays immediate, the completed filter remains the current two-state toggle, and todo loading remains fire-and-forget from the event handlers.

## User Stories

1. As a todo app user, I want the search control to keep filtering todos as I type, so that I can quickly narrow the visible list.
2. As a todo app user, I want the completed filter button to keep toggling the visible completion state, so that I can switch between completion views without learning a new interaction.
3. As a todo app user, I want the toolbar search control to behave the same after the refactor, so that existing workflows do not change.
4. As a todo app user, I want the filter bar search control to behave the same after the refactor, so that filtering remains predictable wherever it appears.
5. As a todo app user, I want the add todo control in the toolbar to keep working, so that filter cleanup does not affect todo creation.
6. As a todo app user, I want the select-all control in the toolbar to keep working, so that bulk selection is not affected by filter cleanup.
7. As a todo app user, I want existing icons, labels, and accessibility behavior to remain intact, so that the interface feels unchanged.
8. As a frontend developer, I want the repeated completed-filter command logic in one place, so that future changes do not require synchronized edits across components.
9. As a frontend developer, I want the repeated search-filter command logic in one place, so that the next filter payload is composed consistently.
10. As a frontend developer, I want filter state to remain owned by the existing filter state hook, so that ADR-0002 remains true.
11. As a frontend developer, I want the new hook to consume the existing todo context directly, so that this change follows the current frontend convention.
12. As a frontend developer, I want the hook to expose a small interface, so that components only need current filters and the relevant command functions.
13. As a frontend developer, I want component rendering code to stay focused on UI structure, so that filter behavior can be understood separately.
14. As a test author, I want to test filter command composition without rendering full UI components, so that the tests are narrower and faster.
15. As a test author, I want to verify calls to filter setters and todo loading, so that the test matches what the new hook actually owns.
16. As a maintainer, I want the extraction to preserve existing runtime behavior, so that this cleanup can ship without product review.
17. As a future agent, I want the scope to exclude debounce, three-state filtering, and context API redesign, so that implementation stays small and low-risk.

## Implementation Decisions

- Build a new deep module for todo filter controls. Its interface should expose current filters, a completed-filter toggle command, and a search-filter update command.
- Modify the toolbar component to consume the new filter controls hook instead of declaring local completed/search handlers.
- Modify the filter bar component to consume the new filter controls hook instead of declaring local completed/search handlers.
- Keep the existing filter state module as the owner of filter state. The new hook must not call local state primitives to store filters.
- Keep the existing context provider as the place where filter state and todo loading are composed. The new hook should consume the existing context API.
- Follow the current direct context-consumption convention. Do not introduce a separate todo-context convenience hook as part of this change.
- Preserve the current two-state completed filter toggle. Do not introduce an all/completed/incomplete tri-state model.
- Preserve immediate search behavior. Do not add debounce, throttling, delayed loading, or request coalescing.
- Preserve fire-and-forget todo loading. Do not make the new command handlers asynchronous, and do not await todo loading.
- Preserve the toolbar’s existing non-filter behavior, including add todo, select all, indeterminate checkbox behavior, icons, labels, and props.
- Preserve the filter bar’s existing visual and accessibility behavior while replacing its duplicated filter command logic.
- Do not create a new ADR. ADR-0002 already records the relevant architectural split.
- Do not update the domain glossary. This is implementation terminology, not product domain language.

## Testing Decisions

- Test the new filter controls hook as the primary required coverage.
- Use Vitest and Testing Library hook rendering, consistent with the frontend test stack.
- Use a minimal todo context provider wrapper with fake filters, fake filter setters, and fake todo loading.
- Assert external command behavior: the completed toggle calls the completed setter with the next boolean and calls todo loading with the merged filter payload.
- Assert external command behavior: the search update calls the search setter with the new text and calls todo loading with the merged filter payload.
- Do not assert that the returned filters mutate after invoking commands. The new hook does not own filter state.
- Do not require new component tests that only mock the new hook. Those tests mostly verify React event wiring rather than filter behavior.
- Keep existing component tests if they already cover visible UI behavior.
- Manual verification should confirm that search filtering, completed filtering, add todo, and select-all behavior still work in the app.

## Out of Scope

- Adding debounced or throttled search.
- Changing completed filtering to a three-state filter.
- Renaming or redesigning the todo context API.
- Introducing a todo-context convenience hook.
- Moving API loading into the filter state hook.
- Adding new filter types.
- Changing backend API contracts.
- Changing visual design, labels, icons, or layout.
- Reworking existing component tests beyond what is needed for this extraction.

## Further Notes

This PRD is based on the refined deepening plan for extracting the frontend filter controls hook. The implementation should treat the extraction as a behavior-preserving cleanup that improves locality and testability without changing the todo product surface.
