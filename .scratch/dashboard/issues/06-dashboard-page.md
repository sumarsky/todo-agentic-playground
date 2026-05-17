Status: ready-for-agent

## Parent

- `.scratch/dashboard/PRD.md`

## What to build

Build the Dashboard page at `/dashboard` that displays per-endpoint metrics as cards.

**Dashboard page** (`/dashboard`):
- Fetches metrics from `GET /api/dashboard/metrics?window=<selected>`
- Time window selector: dropdown with options `1h`, `24h` (default), `7d`, `30d`
- Renders one card per endpoint showing:
  - Endpoint name (method + path, e.g., `POST /todos`)
  - Failure count
  - Average duration (ms)
  - Total requests
- Cards displayed in a responsive grid layout
- Changing the time window triggers a re-fetch (no full page reload)
- Empty state: when no data exists for the selected window, show "No data for this time period"

**Tests**: Component tests following the Vitest + Testing Library pattern:
- Renders cards when data is present
- Renders empty state when no data
- Time window selector triggers re-fetch with correct window param
- Card displays correct endpoint name, failure count, avg duration, total requests

## Acceptance criteria

- [ ] `/dashboard` route renders the Dashboard page component
- [ ] Fetches metrics from `GET /api/dashboard/metrics`
- [ ] Time window dropdown with options: 1h, 24h (default), 7d, 30d
- [ ] One card per endpoint with failure count, avg duration, total requests
- [ ] Cards displayed in responsive grid
- [ ] Changing time window triggers re-fetch without page reload
- [ ] Empty state shown when no data for selected window
- [ ] Component tests cover rendering, time window change, and empty state
- [ ] All existing frontend tests still pass

## Blocked by

- #05-dashboard-metrics-api
