# Issue: Backend - Minimal API scaffolding

## What to build

Set up .NET 10 ASP.NET Core Minimal API project with global error handling and CORS configuration. This is the foundation for all backend work.

**End-to-end**: Create a new .NET 10 project, configure CORS to allow `http://localhost:3000`, set up a global exception handler middleware that returns JSON error responses with HTTP status codes (400, 404, 500).

## Acceptance criteria

- [ ] .NET 10 ASP.NET Core project created and builds successfully
- [ ] CORS middleware configured to allow requests from `http://localhost:3000`
- [ ] Global exception handler returns JSON errors with appropriate HTTP status codes
- [ ] Project runs on `http://localhost:5000`

## Blocked by

None - can start immediately
