Status: done

# 01-postgres-foundation-migration-runner

## Parent

[.scratch/implement-postgres-PRD.md](../implement-postgres-PRD.md)

## What to build

Create the `BackendApi.Storage.Postgres` project with Dapper + Npgsql dependencies. Register `NpgsqlDataSource` as a singleton in DI, reading the connection string from `ConnectionStrings:Default` (with environment variable override support). Build a migration runner that reads all `*.sql` files from a `Migrations/` folder, sorts them by filename, and executes each on startup. Include an idempotent SQL script that creates the `todos` table (uuid id, text title NOT NULL, boolean completed NOT NULL DEFAULT false, timestamp with time zone created_at NOT NULL) using `CREATE TABLE IF NOT EXISTS`. Provide an `AddPostgresStorage(this IServiceCollection, IConfiguration)` extension method for DI wiring. Add an integration test using Testcontainers.PostgreSql that starts the app, confirms the migration runner executes, and verifies the `todos` table exists.

## Acceptance criteria

- [ ] `BackendApi.Storage.Postgres` project builds with Dapper and Npgsql packages
- [ ] `NpgsqlDataSource` registered as singleton from `ConnectionStrings:Default`
- [ ] `AddPostgresStorage` extension method available for DI registration
- [ ] Migration runner executes `*.sql` scripts from `Migrations/` folder on startup
- [ ] `001-create-todos-table.sql` creates the `todos` table idempotently
- [ ] Integration test spins up Testcontainers.PostgreSql and verifies table exists after startup
- [ ] `appsettings.json` in BackendApi has `ConnectionStrings:Default` placeholder

## Blocked by

None - can start immediately
