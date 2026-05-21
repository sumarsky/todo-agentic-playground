# Project Context

This repo is a full-stack Todo application used to exercise a small, contract-driven architecture.

## Product

Users can manage todos through a React UI backed by an ASP.NET Core Minimal API.

Core capabilities:

- Create todos with a required non-empty title.
- List todos.
- Filter todos by completion state.
- Search todos by title text.
- Update todo titles.
- Toggle completion.
- Delete one todo.
- Bulk delete todos by ID.

## Architecture

The backend follows a layered shape:

- `BackendApi/Domain`: core `Todo` model and invariants.
- `BackendApi/Application`: use cases, DTOs, repository port.
- `BackendApi/Infrastructure`: in-memory repository implementation.
- `BackendApi/Program.cs`: HTTP endpoints, CORS, error handling, dependency wiring.
- `BackendApi.Tests`: backend domain, application, infrastructure, API integration, and docs-contract tests.

The frontend lives in `frontend/`:

- React 19 app built with Vite.
- Tailwind/PostCSS styling.
- Vitest component/context tests.
- Playwright e2e tests.

Primary contract source:

- `docs/api-contract.md`

## Domain Model
Domain models are immutable. State changes return new instances (copy-on write).

`Todo` is an immutable record. State changes return new instances (copy-on-write).

`Todo` fields:

- `id`: UUID generated on creation.
- `title`: required non-whitespace string.
- `completed`: boolean, defaults to `false`.
- `createdAt`: UTC timestamp set on creation.

Invariants:

- Title cannot be null, empty, or whitespace.
- New todos start incomplete.
- Completion changes through toggle behavior.
- Updating title preserves identity, completion state, and creation time.
- Missing-id deletes are idempotent at repository/use-case boundary.
- `Todo` instances are immutable — `WithTitle()` and `ToggleCompleted()` return new instances.

## API

Frontend API base URL is the base origin the React frontend uses for backend HTTP requests, including todos, dashboard metrics, and logs.

Frontend dev base URL:

```text
http://localhost:5000
```

Endpoints:

- `GET /health`
- `POST /todos`
- `GET /todos?completed={bool}&search={text}`
- `PUT /todos/{id}/title`
- `PUT /todos/{id}/toggle`
- `DELETE /todos/{id}`
- `DELETE /todos?ids={id1},{id2}`

Todo errors use `ErrorResponse`:

```json
{
  "statusCode": 400,
  "message": "Title cannot be empty or null"
}
```

Unknown routes return:

```json
{ "error": "Not found" }
```

Unhandled exceptions return:

```json
{ "error": "<message>" }
```

## Key Decisions

- API and frontend behavior should stay aligned with `docs/api-contract.md`.
- "Solution split" means splitting backend .NET layers into separate projects while leaving the React frontend unchanged.
- Backend split projects keep the `BackendApi.*` naming prefix for now.
- Namespaces should match project boundaries; API HTTP request/response types live under `BackendApi.Contracts`.
- Repository is intentionally in-memory for current scope.
- Persistence can change behind `ITodoRepository`.
- Repository ports belong in the application layer under `Ports`; domain model stays persistence-free.
- Domain rules belong in the domain model.
- Application orchestration belongs in application use cases/services.
- Use cases stay as concrete classes; do not add one-method interfaces unless a real second implementation appears.
- Do not add clock or ID generator ports during the split; introduce them only when deterministic time or ID generation becomes a real need.
- API endpoints call application use cases directly; avoid a facade that only forwards to use cases.
- HTTP request/response DTOs and HTTP mappers belong in the API layer, not application.
- API layer should map HTTP requests/responses and keep business rules out.
- API contract docs should focus on REST behavior and dependency rules; ADRs own project-split details.
- Dependency injection registration should live in layer extension methods and be called by the API composition root.
- Application and infrastructure DI extensions are named `AddApplication()` and `AddInfrastructure()`.
- Infrastructure exposes `AddInfrastructure` for app wiring while keeping concrete adapters public for direct infrastructure tests.
- Backend tests stay in one project with folders per layer until test dependencies or runtime needs justify splitting.
- Backend test project may reference all backend layer projects so layer-specific tests stay narrow and direct.

## Commands

Backend tests:

```powershell
dotnet test
```

Frontend unit tests:

```powershell
npm test --prefix frontend
```

Frontend lint:

```powershell
npm run lint --prefix frontend
```

Frontend e2e tests:

```powershell
npm run test:e2e --prefix frontend
```

Frontend dev server:

```powershell
npm run dev --prefix frontend
```

## Agent Docs

Engineering skill configuration:

- `AGENTS.md`
- `docs/agents/issue-tracker.md`
- `docs/agents/triage-labels.md`
- `docs/agents/domain.md`

Issues and PRDs live in `.scratch/`.

## Observability

OpenTelemetry console observability via `Microsoft.Extensions.Telemetry`:
- **Logs**: JSON format, enriched with trace ID + HTTP method, path, status code.
- **Traces**: HTTP request spans only (auto-instrumentation) for log correlation.
- **Metrics**: HTTP request metrics (count, duration, status code) exported to console every 5 seconds.
- Setup lives in `BackendApi/Observability/` as a named extension method called by the composition root.
- Global exception handler emits structured error logs with trace ID and exception details.
