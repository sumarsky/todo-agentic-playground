# Centralize the Frontend API Base URL

Status: ready-for-agent

## Parent

.scratch/frontend-api-config/PRD.md

## What to build

Centralize the Frontend API base URL so every frontend backend request uses one shared configuration rule. Todo API calls, dashboard metrics calls, and logs calls should all use the same base origin. The frontend should use `VITE_API_URL` when it is provided at build time and fall back to the existing local backend origin when it is not.

Document the optional build-time setting with an example environment file. Keep this as a narrow configuration extraction: do not refactor dashboard metrics or logs into the Todo API client, do not add runtime browser configuration, and do not introduce separate API origins for observability endpoints.

## Acceptance criteria

- [ ] A shared frontend configuration module exposes the Frontend API base URL as a stable constant.
- [ ] The shared constant uses `VITE_API_URL` when present and falls back to `http://localhost:5000` when absent.
- [ ] Todo API calls consume the shared Frontend API base URL instead of owning the base URL rule locally.
- [ ] Dashboard metrics calls consume the shared Frontend API base URL instead of hardcoding the local backend origin.
- [ ] Logs calls consume the shared Frontend API base URL instead of hardcoding the local backend origin.
- [ ] An example environment file documents the optional `VITE_API_URL` setting without adding committed development or production environment files.
- [ ] Focused unit coverage verifies both the fallback URL and the `VITE_API_URL` override behavior.
- [ ] Existing API client tests continue to verify request URL construction and request behavior.
- [ ] Frontend unit tests pass.
- [ ] Frontend lint passes.

## Blocked by

None - can start immediately
