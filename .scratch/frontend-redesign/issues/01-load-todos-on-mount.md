## 01 - Load todos on mount

**Status:** done

### Description

`TodoContextProvider` currently initializes with an empty `todos` array and never fetches on startup. Add a `useEffect` that calls `listTodos()` when the component mounts so todos are loaded immediately.

### Acceptance Criteria

- On app load, todos are fetched from the API and displayed without user interaction.
- Existing CRUD operations continue to work.

### Testing

- Unit test: verify `listTodos()` is called once on mount.
- Integration test: app renders with todos populated.
