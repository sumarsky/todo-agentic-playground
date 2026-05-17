Status: ready-for-agent

## Parent

- `.scratch/dashboard/PRD.md`

## What to build

Add client-side routing to the frontend and build the Logs page.

**Routing**: Add `react-router-dom` dependency. Restructure `App.jsx` to wrap content in `<BrowserRouter>` with routes for `/`, `/logs`, and `/dashboard` (dashboard page can be a placeholder for now).

**Logs page** (`/logs`):
- Fetches logs from `GET /api/logs` with optional `level` and `message` query params
- Filter controls at the top:
  - Dropdown for level: All, Info, Warning, Error
  - Text input for message search
- Logs displayed in a list/table, ordered by timestamp descending (newest first)
- Each row shows: timestamp, level (color-coded), source, message, and HTTP details (method, path, status) when available
- All matching logs loaded at once — no pagination
- Filters update query params and trigger re-fetch
- Both filters are combinable

**Tests**: Component tests following the Vitest + Testing Library pattern in `frontend/src/components/`:
- Renders log entries when data is present
- Renders empty state when no logs exist
- Level dropdown filters correctly
- Message search filters correctly
- Combined filters work

## Acceptance criteria

- [ ] `react-router-dom` installed and configured
- [ ] `App.jsx` restructured with `<BrowserRouter>` and routes
- [ ] `/logs` route renders the Logs page component
- [ ] `/dashboard` route exists (placeholder is fine)
- [ ] `/` route preserves existing TodoList functionality
- [ ] Logs page fetches from `GET /api/logs`
- [ ] Level dropdown filter works (All, Info, Warning, Error)
- [ ] Message text search filter works
- [ ] Both filters are combinable
- [ ] Logs ordered by timestamp descending
- [ ] Level is color-coded in the display
- [ ] Component tests cover rendering, filtering, and empty state
- [ ] All existing frontend tests still pass

## Blocked by

- #03-logs-api-endpoint
