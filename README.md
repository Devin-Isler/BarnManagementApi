# Barn Management API

A .NET 8 Web API for managing farms, animals, products, and users with JWT-based authentication, Entity Framework Core (SQL Server), AutoMapper, Serilog logging, and background services for domain automation.

## Features
- Farm, Animal, Product, and User management
- JWT authentication with blacklist checks for tokens and users
- EF Core with separate auth and domain contexts
- AutoMapper for DTO ↔ domain mapping
- Serilog request and file logging
- Hosted background services for animal lifecycle and product generation
- Swagger (OpenAPI) with JWT auth support

## Tech Stack
- .NET 8, ASP.NET Core Web API
- Entity Framework Core (SQL Server)
- ASP.NET Core Identity
- AutoMapper, Serilog
- Swagger (Swashbuckle)

## Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server instance
  - Easiest: run SQL Server in Docker, mapped to port 1434 to match the default configuration

```bash
# Example: SQL Server 2022 on a custom port 1434
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong!Passw0rd" \
  -p 1434:1433 --name mssql-2022 -d mcr.microsoft.com/mssql/server:2022-latest
```

### Clone and Restore
```bash
git clone <your-repo-url>
cd BarnManagementApi
dotnet restore
```

## Configuration
Configuration is read from `appsettings.json` and environment variables.

### Connection Strings
`Program.cs` expects two SQL Server connections:
- `ConnectionStrings:BarnConnection` – domain data
- `ConnectionStrings:BarnAuthConnection` – ASP.NET Identity data

Example (do not commit real secrets):
```json
{
  "ConnectionStrings": {
    "BarnConnection": "Server=localhost,1434;Database=BarnDb;User Id=sa;Password=<STRONG_PASSWORD>;Encrypt=True;TrustServerCertificate=True",
    "BarnAuthConnection": "Server=localhost,1434;Database=BarnAuthDb;User Id=sa;Password=<STRONG_PASSWORD>;Encrypt=True;TrustServerCertificate=True"
  }
}
```

You can override via environment variables:
- `ConnectionStrings__BarnConnection`
- `ConnectionStrings__BarnAuthConnection`

### JWT Settings
`Program.cs` reads the following keys:
- `Jwt:Key` (HMAC secret)
- `Jwt:Issuer`
- `Jwt:Audience`

Example:
```json
{
  "Jwt": {
    "Key": "<LONG_RANDOM_SECRET>",
    "Issuer": "https://localhost:7247",
    "Audience": "https://localhost:7247"
  }
}
```

Environment variable overrides:
- `Jwt__Key`, `Jwt__Issuer`, `Jwt__Audience`

> Important: Never commit real secrets. Use secret managers or CI/CD vaults.

## Database Migrations
This project uses two DbContexts: `BarnDbContext` (domain) and `BarnAuthDbContext` (identity).

Apply existing migrations:
```bash
# Domain
dotnet ef database update --context BarnDbContext
# Identity
dotnet ef database update --context BarnAuthDbContext
```

Create new migrations when changing models:
```bash
# Domain
dotnet ef migrations add <Name> --context BarnDbContext
# Identity
dotnet ef migrations add <Name> --context BarnAuthDbContext --output-dir Migrations/AuthMigrations
```

> Ensure the correct startup project and working directory are set. If needed, use `--project` and `--startup-project` flags.

## Run the API
```bash
dotnet run
```

By default, profiles in `Properties/launchSettings.json` expose:
- HTTP: `http://localhost:5004`
- HTTPS: `https://localhost:7247`

Swagger UI: `https://localhost:7247/swagger`

## Authentication
- Register and login via `AuthController` to obtain a JWT.
- Include the token in requests: `Authorization: Bearer <token>`
- Tokens and users are checked against a blacklist via `ITokenRepository` during JWT validation.

## High-level Endpoints
- `AuthController`: register, login, logout/blacklist
- `UserController`: user CRUD
- `FarmController`: farms CRUD
- `AnimalController`: animals CRUD, status updates
- `ProductController`: products CRUD, sales

Explore exact routes and schemas via Swagger.

## Example Requests

### Register
```bash
curl -k -X POST "https://localhost:7247/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "farmer1",
    "email": "farmer1@example.com",
    "password": "StrongP@ss1"
  }'
```

### Login
```bash
curl -k -X POST "https://localhost:7247/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "farmer1",
    "password": "StrongP@ss1"
  }'
# => { token: "<JWT>", expires: "..." }
```

### Authorized request (example: list farms)
```bash
TOKEN="<JWT>"
curl -k "https://localhost:7247/api/farms" \
  -H "Authorization: Bearer $TOKEN"
```

## Logging
Serilog logs to console and to rolling files at `Logs/BarnLog.txt` with per-request timing.

## Background Services
Two hosted services run automatically:
- `ProductService`: generates products from animals
- `AnimalServices`: manages animal lifecycle events

## Development Tips
- Keep DTOs and domain models in sync with AutoMapper profiles (`Mapping/MappingProfile.cs`).
- When changing models, create and apply migrations for both contexts.
- Use environment variables locally to avoid committing secrets.
