# Frontend API Configuration PRD

Status: ready-for-agent

## Problem Statement

The React frontend uses the backend through a Frontend API base URL. That base URL is currently owned in more than one place: the Todo API client already has a Vite environment fallback, while dashboard metrics and logs still hardcode the local backend origin directly.

This makes the frontend harder to deploy outside local development because changing the backend origin requires finding every caller. It also weakens testability because the API origin is not a small, named module with a focused contract.

## Solution

Centralize the Frontend API base URL into a small frontend configuration module. The module exposes one stable constant used by Todo API calls, dashboard metrics calls, and logs calls.

The value comes from Vite build-time configuration. If `VITE_API_URL` is set, the frontend uses that value. If it is unset, the frontend falls back to `http://localhost:5000`, which remains the local development backend origin.

Document the optional environment variable with an example environment file. Do not add committed environment-specific files for development or production.

## User Stories

1. As a frontend developer, I want one canonical Frontend API base URL, so that I do not have to update multiple files when the backend origin changes.
2. As a frontend developer, I want Todo API calls to use the canonical Frontend API base URL, so that todo behavior stays aligned with frontend configuration.
3. As a frontend developer, I want dashboard metrics calls to use the canonical Frontend API base URL, so that dashboard behavior can follow the same deployment configuration as todos.
4. As a frontend developer, I want logs calls to use the canonical Frontend API base URL, so that observability screens do not keep their own hardcoded backend origin.
5. As a developer running the app locally, I want the frontend to keep working when no environment variable is configured, so that local setup remains simple.
6. As a developer running the app locally, I want the default backend origin to remain `http://localhost:5000`, so that existing local backend and frontend commands keep working.
7. As a developer deploying the frontend, I want to provide `VITE_API_URL` at build time, so that the frontend can target staging or production backends without source-code changes.
8. As a developer reviewing configuration, I want the environment variable documented in an example file, so that the deployment knob is discoverable.
9. As a maintainer, I want no committed fake production API URL, so that example values do not accidentally become real build configuration.
10. As a maintainer, I want no empty environment files committed, so that the repo does not accumulate config files that have no runtime effect.
11. As a test author, I want the API configuration rule covered by a focused unit test, so that config behavior can be verified without coupling it to Todo API request tests.
12. As a test author, I want Todo API client tests to keep asserting request URLs and request behavior, so that existing API client coverage remains meaningful.
13. As a future agent, I want the scope to stay limited to configuration extraction, so that dashboard and logs fetch behavior is not refactored incidentally.
14. As a future agent, I want a simple API configuration interface, so that callers can import the base URL without depending on environment details.
15. As a maintainer, I want this change to avoid a runtime config system, so that the app does not gain deployment complexity before it needs it.
16. As a maintainer, I want this change to avoid deriving production configuration from the browser location, so that same-origin deployment is not assumed silently.
17. As a future deployer, I want CI/CD documentation or examples to mention `VITE_API_URL`, so that frontend builds can be configured intentionally.
18. As a code reviewer, I want the old hardcoded dashboard and logs origins removed, so that the frontend has one obvious place for backend origin configuration.

## Implementation Decisions

- Build a frontend API configuration module as a deep module with a very small interface: one exported constant named for the Frontend API base URL.
- The Frontend API base URL is the base origin the React frontend uses for backend HTTP requests, including todos, dashboard metrics, and logs.
- Use build-time Vite configuration. `VITE_API_URL` takes precedence.
- Use `http://localhost:5000` as the fallback when `VITE_API_URL` is unset.
- Do not add a getter function. The value is a stable module-level configuration constant, not mutable runtime state.
- Do not derive production configuration from the browser location. Same-origin frontend/backend deployment is not an established requirement.
- Do not introduce a separate observability API base URL. Dashboard metrics and logs use the same Frontend API base URL as Todo API calls.
- Update the existing Todo API client so it consumes the shared configuration module instead of owning the base URL rule locally.
- Update dashboard metrics and logs callers so they consume the shared configuration module instead of hardcoding the local backend origin.
- Add one example environment file documenting the optional `VITE_API_URL` variable.
- Do not commit development, production, or empty environment files.
- Do not move dashboard metrics or logs fetch logic into the Todo API client as part of this PRD.
- Do not introduce an ADR for this change. The decision is conventional, easy to reverse, and visible in the configuration module plus example environment file.

## Testing Decisions

- Good tests should verify externally visible behavior: what base URL is selected and what URLs callers pass to `fetch`. They should not assert internal module structure beyond the public config export.
- Add focused unit coverage for the frontend API configuration module.
- The config test should verify that the localhost fallback is used when `VITE_API_URL` is unset.
- The config test should verify that `VITE_API_URL` is used when present.
- Keep Todo API client tests focused on request construction, response parsing, and error behavior.
- Existing Todo API client tests are prior art for stubbing `fetch`, resetting modules when environment changes, and asserting concrete request URLs.
- Dashboard metrics and logs should continue to be covered at the component behavior level if tests exist or are added nearby.
- Run the frontend unit test suite after implementation.
- Run frontend lint after implementation because the change adds imports and a new module.

## Out of Scope

- Moving dashboard metrics calls into the Todo API client or a broader frontend API client.
- Moving logs calls into the Todo API client or a broader frontend API client.
- Creating a runtime configuration file loaded by the browser.
- Adding separate base URLs for Todo API and observability endpoints.
- Changing backend routes, CORS, API contracts, or observability behavior.
- Changing the frontend dev server setup.
- Adding committed environment-specific files for development, staging, or production.
- Creating an ADR.

## Further Notes

This PRD refines the existing deepening note for extracting frontend API configuration. It should be implemented as a narrow configuration extraction: no user-facing UI changes, no backend behavior changes, and no broader API client refactor.
