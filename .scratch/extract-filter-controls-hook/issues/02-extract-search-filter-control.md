# Extract Search Filter Control

Status: done

## Parent

.scratch/extract-filter-controls-hook/PRD.md

## What to build

Extend the todo filter controls hook with the search-filter command and route search input changes through it wherever duplicated search command logic currently exists. Search should continue to update immediately on input changes and should continue to trigger todo loading with the merged filter payload.

This slice should preserve existing behavior rather than introducing debounce, throttling, delayed loading, or a new context API.

## Acceptance criteria

- [x] The todo filter controls hook exposes a search-filter update command.
- [x] The search-filter update command calls the existing search-filter setter with the provided text.
- [x] The search-filter update command calls todo loading with the current filters merged with the next search value.
- [x] Toolbar search input changes use the shared search command without changing toolbar add, select-all, icon, label, or accessibility behavior.
- [x] Filter bar search input changes use the shared search command without changing its visual or accessibility behavior.
- [x] Search remains immediate and fire-and-forget.
- [x] Focused hook tests verify the search setter call and todo-loading payload using a minimal context provider wrapper.

## Blocked by

- .scratch/extract-filter-controls-hook/issues/01-extract-completed-filter-control.md
