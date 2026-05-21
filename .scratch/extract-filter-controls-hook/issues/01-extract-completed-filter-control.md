# Extract Completed Filter Control

Status: done

## Parent

.scratch/extract-filter-controls-hook/PRD.md

## What to build

Create the todo filter controls hook and route completed-filter toggling through it wherever the duplicated completed-filter command currently exists. The completed filter should continue to use the existing two-state toggle and should keep using the existing todo context as the source of filters, filter setters, and todo loading.

This slice should preserve the existing filter state ownership: the existing filter state hook owns state, the context provider composes state with loading, and the new controls hook only coordinates commands.

## Acceptance criteria

- [x] A reusable todo filter controls hook exposes the current filters and a completed-filter toggle command.
- [x] The completed-filter toggle command calls the existing completed-filter setter with the next boolean value.
- [x] The completed-filter toggle command calls todo loading with the current filters merged with the next completed value.
- [x] The hook does not create or own independent filter state.
- [x] Components that previously duplicated completed-filter toggling now use the shared command.
- [x] Existing completed-filter behavior remains fire-and-forget and does not await todo loading.
- [x] Focused hook tests verify the completed-filter setter call and todo-loading payload using a minimal context provider wrapper.

## Blocked by

None - can start immediately
