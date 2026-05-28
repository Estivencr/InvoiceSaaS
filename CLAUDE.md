# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

### Backend (ASP.NET Core 8)

```bash
# Run API (from repo root)
dotnet run --project src/InvoiceSaaS.Api

# Run with watch (hot reload)
dotnet watch run --project src/InvoiceSaaS.Api

# Run all tests
dotnet test

# Run a single test project
dotnet test tests/InvoiceSaaS.Application.Tests/

# Run a single test class
dotnet test tests/InvoiceSaaS.Application.Tests/ --filter "FullyQualifiedName~AuthServiceTests"

# EF Core migrations
dotnet ef migrations add <MigrationName> --project src/InvoiceSaaS.Infrastructure --startup-project src/InvoiceSaaS.Api
dotnet ef database update --project src/InvoiceSaaS.Infrastructure --startup-project src/InvoiceSaaS.Api

# Build solution
dotnet build InvoiceSaaS.sln
```

### Frontend (React + TypeScript)

```bash
cd frontend/invoice-saas-ui
npm install
npm start         # Dev server on :3000
npm test
npm run build
```

### Docker

```bash
cp .env.example .env                              # First-time setup
docker-compose up -d                              # Start all (postgres + api + frontend)
docker-compose logs -f api                        # Follow API logs
docker-compose restart api                        # Restart after changes
docker-compose down -v                            # Reset (deletes DB data)
docker-compose --profile production up -d         # With Nginx reverse proxy
```

## Architecture

**Clean Architecture** — strict dependency rule: Domain ← Application ← Infrastructure ← Api.

```
InvoiceSaaS.Domain/          ← No dependencies. Entities, IRepository<T>, IUnitOfWork, Enums.
InvoiceSaaS.Application/     ← References Domain only. Services, DTOs, Validators (FluentValidation), AutoMapper profiles.
InvoiceSaaS.Infrastructure/  ← References Application + Domain. EF Core DbContext, GenericRepository<T>, UnitOfWork, JwtTokenService, CurrentUserService.
InvoiceSaaS.Api/             ← References all. Controllers, Middleware, Program.cs, DI wiring.
```

### Multi-tenancy

Every entity that belongs to a company has a `CompanyId` field. `CurrentUserService` extracts `company_id` from JWT claims. All service methods receive `companyId` as an explicit parameter and filter by it — never use queries without a CompanyId filter.

EF Core `HasQueryFilter` on `User`, `Customer`, `Employee`, and `Invoice` automatically adds `WHERE is_deleted = false`.

### Repository Pattern

`IUnitOfWork.Repository<T>()` returns a `GenericRepository<T>`. For complex queries that need `Include()` or projections, call `.Query()` and chain LINQ on top. Repositories are scoped and live inside `UnitOfWork`.

### JWT Flow

1. `POST /api/auth/login` → `AuthService` verifies BCrypt hash, calls `JwtTokenService.GenerateAccessToken()` and `GenerateRefreshToken()`, stores refresh token in `users.refresh_token`.
2. Access token: 15 min, HS256, claims: `sub`, `email`, `company_id`, `role` (multiple).
3. `POST /api/auth/refresh` → validates refresh token from DB, issues new token pair (rotation).
4. `CurrentUserService` reads claims from `IHttpContextAccessor`.

### Invoice Business Rules

- `InvoiceNumber` format: `INV-YYYY-MM-XXXXX` — auto-incremented per company per month (see `InvoiceService.GenerateInvoiceNumberAsync`).
- State machine: `Pending → Paid` or `Pending → Cancelled` only. Paid invoices are immutable.
- `Invoice.RecalculateTotals()` must be called after any detail change.
- Minimum 1 detail line required to create or update an invoice.

### Error Handling

`ExceptionMiddleware` catches all exceptions and maps them to `ApiResponse.Fail(...)`. Use:
- `BusinessException` (400) for domain rule violations.
- `NotFoundException` (404) for missing entities.
- `UnauthorizedException` (401) for auth failures.
- `ForbiddenException` (403) for authorization failures.
- `FluentValidation.ValidationException` is automatically caught and returns 400 with field-level errors.

All API responses use `ApiResponse<T>` with `success`, `message`, `data`, `errors`, `timestamp`.

## Key Files

| File | Purpose |
|------|---------|
| `src/InvoiceSaaS.Api/Program.cs` | App bootstrap, middleware pipeline, DI wiring |
| `src/InvoiceSaaS.Api/Extensions/ServiceCollectionExtensions.cs` | All DI registrations |
| `src/InvoiceSaaS.Infrastructure/Data/InvoiceSaasDbContext.cs` | EF Core context |
| `src/InvoiceSaaS.Infrastructure/Data/Configurations/` | Fluent API table mappings (snake_case column names) |
| `src/InvoiceSaaS.Application/Services/InvoiceService.cs` | Core billing logic |
| `src/InvoiceSaaS.Application/Services/AuthService.cs` | JWT auth, account lockout |
| `src/InvoiceSaaS.Application/Mappings/MappingProfile.cs` | AutoMapper config |
| `src/InvoiceSaaS.Infrastructure/Repositories/UnitOfWork.cs` | Transaction management |
| `frontend/invoice-saas-ui/src/context/AuthContext.tsx` | React auth state + JWT storage |
| `frontend/invoice-saas-ui/src/services/api.ts` | Axios instance with JWT interceptor + refresh logic |
| `docker-compose.yml` | Local dev environment |

## Conventions

- DB column names use `snake_case` (mapped in Fluent API configurations).
- C# properties use `PascalCase`.
- Soft deletes only — set `IsDeleted = true`, never call `Remove()` on domain entities.
- Service method signatures: `(Guid companyId, ..., CancellationToken ct = default)`.
- All controllers are `[Authorize]` by default; use `[Authorize(Roles = "Admin")]` for admin-only.
- Frontend `localStorage` keys: `accessToken`, `refreshToken`, `user` (JSON).
