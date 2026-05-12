# Issue: Integration - End-to-end flow tests

## What to build

Write integration tests across frontend and backend to verify full user flows work end-to-end. This validates the complete system.

**End-to-end**:

1. **Backend integration tests**: Create → List → Update → Delete flow; verify state changes and repository calls
2. **Frontend integration tests**: Full user flow (add todo → view list → filter → update → delete → bulk delete)
3. **System integration**: Start both backend and frontend servers, test real HTTP calls and UI interactions
4. Verify error scenarios (empty title, non-existent id, network failures)

## Acceptance criteria

- [ ] Backend integration tests pass (use cases orchestrated correctly, repository state correct)
- [ ] Frontend integration tests pass (components interact, context updates correctly)
- [ ] System integration tests pass (full flow with real API calls)
- [ ] Error scenarios tested (validation errors, 404s, etc.)
- [ ] All frontend-backend handshake verified (request/response shape matches)

## Blocked by

#005-backend-usecases-endpoints
#009-frontend-filters
