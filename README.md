# InvoiceSaaS

Enterprise Invoice Management SaaS — ASP.NET Core 8 + React + PostgreSQL + Docker.

## Quick Start (Docker)

```bash
cp .env.example .env
docker-compose up -d
```

Wait ~30 seconds for migrations to run, then open:
- **API Swagger:** http://localhost:5000/swagger
- **Frontend:** http://localhost:3000
- **Health:** http://localhost:5000/health

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend | ASP.NET Core 8, Clean Architecture (4 layers) |
| ORM | Entity Framework Core 8 + Npgsql |
| Database | PostgreSQL 15 |
| Auth | JWT (15 min) + Refresh Tokens (7 days), BCrypt |
| Validation | FluentValidation |
| Mapping | AutoMapper |
| Logging | Serilog (console + file rotation) |
| Frontend | React 18 + TypeScript + Material-UI |
| HTTP Client | Axios with JWT interceptors |
| Testing | xUnit + Moq + FluentAssertions |
| Container | Docker Compose |
| Proxy | Nginx |

## Architecture

```
Clean Architecture — 4 layers:
  Domain          → Entities, Interfaces, Enums (no dependencies)
  Application     → Services, DTOs, Validators, Mappings
  Infrastructure  → EF Core, Repositories, JWT, Password hashing
  Api             → Controllers, Middleware, Program.cs
```

Multi-tenancy is implemented via `CompanyId` row-level filtering — every query is scoped to the JWT claim `company_id`.

## API Endpoints

### Auth
| Method | Route | Description |
|--------|-------|-------------|
| POST | /api/auth/register | Register company + admin user |
| POST | /api/auth/login | Login, returns JWT + refresh token |
| POST | /api/auth/refresh | Refresh access token |
| POST | /api/auth/logout | Invalidate refresh token |
| POST | /api/auth/change-password | Change current user's password |

### Security (Admin only)
| Method | Route |
|--------|-------|
| GET | /api/security/users |
| POST | /api/security/users |
| PUT | /api/security/users/{id} |
| DELETE | /api/security/users/{id} |
| PATCH | /api/security/users/{id}/change-role |
| PATCH | /api/security/users/{id}/toggle-status |

### Customers, Employees, Invoices, Reports
Standard CRUD following the same pattern. See Swagger at `/swagger` for full documentation.

## Running Locally Without Docker

Requirements: .NET 8 SDK, Node 18, PostgreSQL 15.

```bash
# Backend
cd src/InvoiceSaaS.Api
dotnet ef database update --project ../InvoiceSaaS.Infrastructure
dotnet run

# Frontend
cd frontend/invoice-saas-ui
npm install
npm start
```

## Running Tests

```bash
dotnet test tests/InvoiceSaaS.Application.Tests/
dotnet test tests/InvoiceSaaS.Api.Tests/
```

## Docker Commands

```bash
docker-compose up -d          # Start all services
docker-compose logs -f api    # Follow API logs
docker-compose restart api    # Restart after code changes
docker-compose down           # Stop services (preserve data)
docker-compose down -v        # Stop + delete volumes (reset DB)

# Run with Nginx (production simulation)
docker-compose --profile production up -d
```

## Environment Variables

Copy `.env.example` to `.env` and set:

| Variable | Description |
|----------|-------------|
| DB_USER / DB_PASSWORD / DB_NAME | PostgreSQL credentials |
| JWT_SECRET | Must be ≥ 32 characters |
| JWT_EXPIRATION_MINUTES | Access token TTL (default: 15) |
| JWT_REFRESH_EXPIRATION_DAYS | Refresh token TTL (default: 7) |

## AWS Free Tier Deployment

1. Launch EC2 `t2.micro` (Amazon Linux 2023)
2. Install Docker: `sudo yum install -y docker && sudo systemctl start docker`
3. Copy project files to EC2
4. Set production `.env` values (change DB_PASSWORD and JWT_SECRET)
5. `docker-compose --profile production up -d`
6. Optional: Point RDS PostgreSQL instance to replace the postgres container

## Security Notes

- Account locks after 5 failed login attempts (15 min lockout)
- All sensitive routes require Bearer JWT
- CompanyId is extracted from JWT claims — users cannot access other companies' data
- Soft deletes only — no hard deletes in production
- Passwords: BCrypt with 10 rounds minimum
