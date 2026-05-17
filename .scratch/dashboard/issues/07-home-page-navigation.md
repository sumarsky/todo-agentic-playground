Status: ready-for-agent

## Parent

- `.scratch/dashboard/PRD.md`

## What to build

Add navigation buttons to the home page so users can discover and access the Dashboard and Logs pages.

**Home page changes** (`/`):
- Add two buttons below the existing `<TodoList />` component:
  - "Dashboard" — links to `/dashboard`
  - "Logs" — links to `/logs`
- Buttons use `react-router-dom`'s `Link` component for client-side navigation
- Buttons are clearly visible and styled consistently with the existing app

**Tests**: Component tests following the Vitest + Testing Library pattern:
- Dashboard button renders and navigates to `/dashboard`
- Logs button renders and navigates to `/logs`
- Existing TodoList functionality is unaffected

## Acceptance criteria

- [ ] "Dashboard" button visible on home page
- [ ] "Logs" button visible on home page
- [ ] Dashboard button navigates to `/dashboard` via client-side routing
- [ ] Logs button navigates to `/logs` via client-side routing
- [ ] Existing TodoList functionality is unchanged
- [ ] Component tests verify button rendering and navigation
- [ ] All existing frontend tests still pass

## Blocked by

- #04-logs-page-with-routing
- #06-dashboard-page
