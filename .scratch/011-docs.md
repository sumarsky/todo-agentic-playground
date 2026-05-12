# Issue: Docs - API contract & domain model

## What to build

Document the API contract, domain model, and testing strategy. This is a design review gate before full implementation.

**End-to-end**: Create documentation summarizing:
- Todo domain model (fields, invariants)
- REST API endpoints (paths, methods, request/response shapes)
- Error handling (status codes, error format)
- Testing strategy (which modules tested, test types)
- Architecture diagram (layers, dependencies)

This document serves as the single source of truth for frontend/backend handshake.

## Acceptance criteria

- [ ] API contract documented (all endpoints, request/response schema)
- [ ] Domain model documented (Todo entity, repository port)
- [ ] Testing strategy documented (test types, module coverage)
- [ ] Architecture diagram included (domain → application → infrastructure)
- [ ] Design review completed (no breaking changes after this point)

## Blocked by

#005-backend-usecases-endpoints
