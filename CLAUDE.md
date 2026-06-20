# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run Commands

```powershell
# Build entire solution
dotnet build TransactionPlatform.slnx

# Run Transactions API locally (port 5299 http, 7041 https)
dotnet run --project src/transactions-service/Transactions.API/Transactions.API.csproj

# Run Payments API locally (port 5295 http, 7286 https)
dotnet run --project src/payments-service/Payments.API/Payments.API.csproj

# Run full stack (RabbitMQ + two SQL Server DBs + both services)
docker-compose up

# EF Core migrations (run from repo root, targeting Transactions.Infrastructure)
dotnet ef migrations add <MigrationName> --project src/transactions-service/Transactions.Infrastructure --startup-project src/transactions-service/Transactions.API
dotnet ef database update --project src/transactions-service/Transactions.Infrastructure --startup-project src/transactions-service/Transactions.API
```

API docs (Scalar UI) are available at `/scalar/v1` on each running service.

## Architecture Overview

This is an **early-stage microservices platform** using Clean Architecture + DDD + event-driven messaging. Two bounded contexts: **Transactions** and **Payments**, each following the same four-layer structure.

### Solution Layout

```
BuildingBlocks/               # Shared infrastructure library
src/transactions-service/
  Transactions.Domain/        # Entities, domain events, enums — no dependencies
  Transactions.Application/   # Use cases / CQRS handlers (scaffolded, empty)
  Transactions.Infrastructure/# EF Core, DbContext, interceptors, DI registration
  Transactions.API/           # ASP.NET Core entry point, controllers
src/payments-service/
  Payments.Domain/            # (empty, ready for implementation)
  Payments.Application/       # (empty)
  Payments.Infrastructure/    # (empty)
  Payments.API/               # Entry point, placeholder controller
docker-compose.yml            # RabbitMQ + two SQL Server instances + both services
```

### Key Patterns

**Transactional Outbox / Inbox**
- `OutboxSaveChangesInterceptor` fires on every `SaveChangesAsync`, inspects the EF `ChangeTracker` for entities implementing `IHasDomainEvents`, serializes their domain events into `OutboxMessage` rows — all within the same database transaction.
- `InboxMessage` + `IdempotencyService` provide at-least-once / deduplicated consumption: before processing a message, check `ExistsAsync(id, consumer)`; after, call `MarkProcessedAsync`.

**Domain Events flow**
1. Aggregate root (e.g. `Transaction`) raises events via the `DomainEvents` collection on `Entity` base class.
2. `OutboxSaveChangesInterceptor` converts them to `OutboxMessage` rows before commit.
3. A background worker (not yet implemented) would poll the outbox and publish to RabbitMQ via MassTransit.

**BuildingBlocks DI extension**
Call `services.AddMessaging(serviceName)` (from `ServiceCollectionExtensions`) to register `CorrelationContext`, `IdempotencyService`, and `OutboxService` in any service's `Program.cs`.

**Correlation**
`CorrelationMiddleware` reads the `X-Correlation-ID` request header (or generates a new UUID) and stores it in scoped `CorrelationContext`. Add it via `app.UseMiddleware<CorrelationMiddleware>()`.

### Data Model Highlights

- `Transaction`: Reference (unique), Amount (decimal 18,2), Currency (default "USD"), Status (Pending/Completed/Failed), IdempotencyKey (unique), audit timestamps, RowVersion.
- `OutboxMessage`: Type, Payload (JSON), ProcessedAt, LockedUntil, RetryCount, RowVersion.
- `InboxMessage`: Composite unique index on (Id, Consumer); prevents duplicate processing per consumer.
- Audit timestamps (`CreatedAt`, `UpdatedAt`) are applied by `BaseEntityConfiguration` to all `IAudiEntity` implementations using `GETUTCDATE()` SQL defaults.

### Infrastructure Notes

- **SQL Server** retry-on-failure is enabled in `ServiceContainer.cs` (`EnableRetryOnFailure`).
- **DesignTimeDbContextFactory** exists in `Transactions.Infrastructure` so EF migrations can run without a running API.
- Local dev connection string targets `DESKTOP-VJSE203\SQLEXPRESS`; Docker uses `transactions-db` / `payments-db` hostnames.
- RabbitMQ management UI: `http://localhost:15672` (guest/guest).

### Adding a New Use Case (Transactions service pattern)

1. Add command/query + handler in `Transactions.Application`.
2. Raise domain events from the aggregate in `Transactions.Domain`.
3. Register any new services in `Transactions.Infrastructure/DependencyInjection/ServiceContainer.cs`.
4. Expose via a controller or minimal-API endpoint in `Transactions.API`.
