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
- `BackendApi/Application`: use cases, application service, DTOs, mapper, repository port.
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

## API

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
- Repository is intentionally in-memory for current scope.
- Persistence can change behind `ITodoRepository`.
- Domain rules belong in the domain model.
- Application orchestration belongs in application use cases/services.
- API layer should map HTTP requests/responses and keep business rules out.

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
