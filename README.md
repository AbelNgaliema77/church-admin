# Church Admin Backend Starter

Senior-level backend starter for the church admin app.

## Architecture

```text
ChurchAdmin.Api
ChurchAdmin.Application
ChurchAdmin.Domain
ChurchAdmin.Infrastructure
ChurchAdmin.Tests
```

## Requirements

- .NET 9 SDK
- SQL Server / SQL Server Express / LocalDB

## Run

```bash
dotnet restore
dotnet build
dotnet run --project src/ChurchAdmin.Api
```

Health check:

```text
https://localhost:xxxx/api/health
```

Swagger:

```text
https://localhost:xxxx/swagger
```

## Database

The API currently auto-applies EF migrations at startup.

Next step:

```bash
dotnet ef migrations add InitialCreate --project src/ChurchAdmin.Infrastructure --startup-project src/ChurchAdmin.Api
dotnet ef database update --project src/ChurchAdmin.Infrastructure --startup-project src/ChurchAdmin.Api
```

## Current coverage

- Clean Architecture projects
- Domain entities
- Soft delete
- Audit-ready base classes
- EF Core DbContext
- SQL Server provider
- Team seed data
- CORS for React local dev

## Next build batch

- TeamsController
- AttendanceController
- WorkersController
- FinanceController
- InventoryController
- Validation layer
- API DTOs
- React API client replacement
