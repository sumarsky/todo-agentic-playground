Status: ready-for-agent

# PRD: Split Backend Into Layered Projects

## Problem Statement

The backend currently keeps domain, application, infrastructure, and API code in one .NET project. The folders already suggest DDD and hexagonal architecture, but project references do not enforce those boundaries, so future changes can accidentally couple the domain model to persistence, HTTP contracts, or composition concerns.

## Solution

Split the backend into separate .NET projects for domain, application, infrastructure, and the existing API host while leaving the React frontend unchanged. The split should preserve the existing REST behavior, keep the API contract stable, and make the architecture boundaries explicit through project references and dependency injection.

## User Stories

1. As a backend developer, I want the domain model in its own project, so that business invariants stay independent from API and persistence concerns.
2. As a backend developer, I want application use cases in their own project, so that Todo workflows are isolated from HTTP routing and infrastructure details.
3. As a backend developer, I want repository ports in the application layer, so that use cases can define their persistence needs without making the domain model persistence-aware.
4. As a backend developer, I want infrastructure adapters in their own project, so that persistence implementations can change behind the application port.
5. As a backend developer, I want the API host to remain the composition root, so that HTTP endpoints, CORS, error handling, and dependency wiring stay at the system edge.
6. As a frontend developer, I want the Todo REST API behavior to remain unchanged, so that the React UI continues to work without client changes.
7. As a maintainer, I want HTTP request and response contract types owned by the API layer, so that transport shapes do not leak into application use cases.
8. As a maintainer, I want API endpoints to call concrete use cases directly, so that there is no forwarding facade hiding the real application boundary.
9. As a maintainer, I want dependency injection registration exposed through layer extension methods, so that the API host can compose layers without knowing every registration detail.
10. As a maintainer, I want concrete use case classes without one-method interfaces, so that the design avoids abstraction that has no second implementation.
11. As a maintainer, I want namespaces to match project boundaries, so that code navigation makes architectural ownership obvious.
12. As a maintainer, I want the existing test suite to keep fast, narrow layer tests, so that refactoring failures point to the right layer.
13. As a maintainer, I want API integration tests to continue exercising the API host, so that the public HTTP contract remains protected.
14. As a maintainer, I want the in-memory repository adapter to remain directly testable, so that infrastructure behavior can be verified without going through HTTP.
15. As a future contributor, I want the ADR to explain why a small Todo app uses multiple backend projects, so that the split is not mistaken for accidental overengineering.
16. As a future contributor, I want the API contract doc to stay focused on REST behavior and dependency rules, so that project naming churn does not obscure the frontend/backend handshake.
17. As a future contributor, I want the split to avoid adding clock or ID generator ports for now, so that this change stays focused on layer boundaries.
18. As a reviewer, I want all existing backend tests to pass after the split, so that the refactor is behavior-preserving.
19. As a reviewer, I want docs updated where they describe obsolete application facade behavior, so that implementation and documentation agree.
20. As an AFK agent, I want the project split decisions captured before implementation, so that I can execute the refactor without reopening settled architecture questions.

## Implementation Decisions

- Create separate backend projects for Domain, Application, Infrastructure, and the existing API host.
- Keep the `BackendApi.*` naming prefix for new backend projects.
- Leave the React frontend unchanged.
- Keep the API host as the ASP.NET Minimal API project and composition root.
- Move the `Todo` domain model and invariants to the domain project.
- Move application use cases to the application project.
- Move repository ports to an application `Ports` namespace/folder.
- Move the repository port out of the domain model so the domain stays persistence-free.
- Move HTTP request/response contract types to the API layer under `Contracts`.
- Move HTTP mapping code to the API layer.
- Remove the forwarding application facade.
- Inject concrete use cases directly into API endpoints.
- Do not create use-case interfaces unless a real second implementation appears later.
- Add `AddApplication()` for application-layer dependency injection registration.
- Add `AddInfrastructure()` for infrastructure dependency injection registration.
- Register the in-memory repository implementation through the infrastructure extension.
- Keep concrete infrastructure adapters public so infrastructure tests can instantiate them directly.
- Match namespaces to project boundaries.
- Do not add clock or ID generator ports during this split.
- Update architecture and testing documentation that currently mentions the application facade.
- Keep the API contract focused on REST behavior and dependency rules; keep project split rationale in the ADR.

## Testing Decisions

- Good tests should assert external behavior of each layer rather than implementation details such as constructor wiring or private helper calls.
- Domain tests should verify Todo invariants, title updates, completion toggling, identity, and creation time behavior.
- Application use-case tests should verify create, list filters, title update, toggle, delete, and bulk delete behavior through the repository port.
- Infrastructure tests should verify the in-memory repository adapter behavior directly.
- API integration tests should verify endpoint status codes, response shapes, lifecycle behavior, fallback 404 behavior, and unhandled exception format.
- Docs-contract tests should continue verifying that the API contract document exists and covers required handshake topics.
- Keep one backend test project with folders per layer.
- Allow the backend test project to reference all backend layer projects so layer-specific tests remain narrow.
- Remove or replace tests for the deleted application facade.
- Run the backend test suite after implementation.
- Frontend tests are not expected to change because the REST API behavior should remain stable.

## Out of Scope

- Changing the React frontend.
- Changing the public Todo REST API behavior.
- Replacing the in-memory repository with a database.
- Renaming the backend from `BackendApi.*` to a domain-specific prefix.
- Adding use-case interfaces.
- Adding clock or ID generator ports.
- Splitting the backend test project into multiple test projects.
- Redesigning error response shapes.
- Introducing CQRS, messaging, event sourcing, or multiple bounded contexts.

## Further Notes

ADR `0001-split-backend-into-layered-projects` records the architectural decision and rationale. The implementation should preserve the existing API contract while making the intended DDD/hexagonal boundaries enforceable through project references.
