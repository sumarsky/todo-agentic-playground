# Split backend into layered projects

The backend will be split into separate .NET projects for `BackendApi.Domain`, `BackendApi.Application`, `BackendApi.Infrastructure`, and the existing `BackendApi` host, while the React frontend stays unchanged. This deliberately adds project and reference overhead to enforce the DDD/hexagonal boundaries: domain stays persistence-free, application owns use cases and ports, infrastructure owns adapters, and the API owns HTTP DTOs, mappers, and composition.

**Consequences**

- The repository port moves to `BackendApi.Application/Ports`.
- Namespaces match project boundaries, and API HTTP request/response types live under `BackendApi.Contracts`.
- Use cases remain concrete classes rather than one-method interfaces.
- Clock and ID generator ports are not added during this split; those abstractions wait until deterministic time or ID generation becomes a real need.
- Layer dependency injection registration lives in `AddApplication()` and `AddInfrastructure()` extension methods called by the API composition root.
- Infrastructure exposes a registration extension for normal app wiring while keeping concrete adapters public for direct infrastructure tests.
- API endpoints call application use cases directly instead of a forwarding facade.
- Backend tests remain in one test project with folders per layer until separate test dependencies or runtimes justify splitting, and that test project may reference every backend layer project for narrow tests.
