# Church Admin — Backend API

.NET 9 / ASP.NET Core / PostgreSQL

## Architecture

```
src/
  ChurchAdmin.Api/           ← Controllers, DTOs, JWT, middleware
  ChurchAdmin.Application/   ← Interfaces (ports)
  ChurchAdmin.Infrastructure/← EF Core, migrations, services
  ChurchAdmin.Domain/        ← Entities, enums, base classes
```

## Environment Variables (required on Render)

| Variable | Description |
|---|---|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string |
| `Auth__JwtKey` | Secret key ≥ 32 chars (Render auto-generates) |
| `Auth__Issuer` | `ChurchAdmin.Api` |
| `Auth__Audience` | `ChurchAdmin.App` |
| `ALLOWED_ORIGINS` | Comma-separated frontend URL(s) |
| `Frontend__BaseUrl` | Public frontend URL used for invite links |
| `Frontend__ProductName` | Product name used in invite emails |
| `EmailSettings__Host` | SMTP host |
| `EmailSettings__Username` | SMTP username |
| `EmailSettings__Password` | SMTP password |
| `EmailSettings__FromEmail` | From address |

## Local Development

```bash
# Start PostgreSQL
docker run -p 5432:5432 -e POSTGRES_PASSWORD=postgres postgres

# Run migrations + start
dotnet run --project src/ChurchAdmin.Api
```

## Deploy on Render

1. Create a **PostgreSQL** database on Render
2. Create a **Web Service** with Docker runtime pointing to this repo
3. Set all required environment variables
4. Deploy — migrations run automatically on startup

## First Login

After first deploy, use `POST /api/auth/login` with:
- Email: `admin@church.local`
- Password: set through the admin invite flow (`/set-password?token=...`)
- ChurchSlug: `laborne`
