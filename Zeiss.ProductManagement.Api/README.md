# ZEISS Product Management - Backend

ASP.NET Core 10 Web API for product catalog and inventory management.

The backend follows Clean Architecture, separating business rules, application logic, infrastructure, API endpoints, background services, and automated tests.

## Technologies

- ASP.NET Core 10
- Entity Framework Core
- SQL Server 2022
- FluentValidation
- Docker
- xUnit

## Project Structure

```text
Zeiss.ProductManagement.Api/
├── Zeiss.ProductManagement.Api/              # HTTP API
├── Zeiss.ProductManagement.Application/      # Use cases and application logic
├── Zeiss.ProductManagement.Domain/           # Entities and business rules
├── Zeiss.ProductManagement.Infrastructure/   # Database and persistence
├── Zeiss.ProductManagement.Worker/           # Background initialization
└── Zeiss.ProductManagement.Tests/            # Automated tests
```

## Requirements

- .NET 10 SDK
- Docker Desktop with Docker Compose
- Node.js 22 (required only for frontend development)

## Local Development

The following setup runs SQL Server in Docker while the API and frontend run locally.

### 1. Start SQL Server

From the backend directory, create your environment file:

```powershell
Copy-Item .env.example .env
```

Set a strong password for `MSSQL_SA_PASSWORD` in `.env`, then start SQL Server:

```powershell
docker compose up -d --wait sqlserver
docker compose ps
```

SQL Server 2022 runs on `localhost,1434`, with data persisted in a Docker volume.

### 2. Configure the database connection

Configure the API connection string using .NET User Secrets.

Run from the backend directory:

```powershell
$passwordLine = Get-Content .\.env | Where-Object { $_ -match '^MSSQL_SA_PASSWORD=' } | Select-Object -First 1

if (-not $passwordLine) {
    throw 'MSSQL_SA_PASSWORD was not found in .env'
}

$env:MSSQL_SA_PASSWORD = $passwordLine -replace '^MSSQL_SA_PASSWORD=', ''

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1434;Database=ZeissProductManagement;User Id=sa;Password=$env:MSSQL_SA_PASSWORD;TrustServerCertificate=True;Encrypt=True;" --project .\Zeiss.ProductManagement.Api

Remove-Item Env:\MSSQL_SA_PASSWORD
```

User Secrets keep local credentials outside the repository.

### 3. Run the API

```powershell
dotnet run --project .\Zeiss.ProductManagement.Api --launch-profile https
```

The API is available at:

- HTTPS: https://localhost:7068
- HTTP: http://localhost:5013

Trust the development certificate if necessary:

```powershell
dotnet dev-certs https --trust
```

During development, the API applies pending migrations and initializes sample data on startup.

### 4. Run the frontend

In another terminal:

```powershell
cd ..\zeiss-product-management-front
npm ci
npm start
```

Open http://localhost:4200.

The frontend communicates with the API through `https://localhost:7068/api`.

## Database Migrations

Create a migration after changing the data model:

```powershell
dotnet ef migrations add MigrationName --project .\Zeiss.ProductManagement.Infrastructure --startup-project .\Zeiss.ProductManagement.Api --output-dir Persistence\Migrations
```

Apply migrations manually:

```powershell
dotnet ef database update --project .\Zeiss.ProductManagement.Infrastructure --startup-project .\Zeiss.ProductManagement.Api
```

Install the EF Core CLI tool if needed:

```powershell
dotnet tool install --global dotnet-ef
```

### Worker

The Worker project can be executed separately to apply migrations and initialize sample data:

```powershell
dotnet run --project .\Zeiss.ProductManagement.Worker
```

When running the Worker independently, provide the database connection through the `ConnectionStrings__DefaultConnection` environment variable.

## Build and Tests

Run from the backend directory:

```powershell
dotnet restore
dotnet build
dotnet test
```

## API Endpoints

| Method | Endpoint                                        | Description                    |
| ------ | ----------------------------------------------- | ------------------------------ |
| GET    | `/api/products?page=1&pageSize=10`              | List products with pagination  |
| POST   | `/api/products`                                 | Create a product               |
| GET    | `/api/products/{id}`                            | Get a product by ID            |
| PUT    | `/api/products/{id}`                            | Update a product               |
| DELETE | `/api/products/{id}`                            | Delete a product               |
| POST   | `/api/products/{id}/add-to-stock/{quantity}`    | Add stock                      |
| POST   | `/api/products/{id}/decrement-stock/{quantity}` | Decrease stock                 |
| GET    | `/api/products/search?name={name}`              | Search products by name        |
| GET    | `/api/products/stock-level?min={min}&max={max}` | Filter products by stock level |

Product listing also supports the `search`, `minStock`, and `maxStock` query parameters.

## Error Handling

The API uses a consistent `BaseResponse` structure.

Validation and business rule errors are returned through `BaseResponse.errors`, containing:

- Entity
- Property (optional)
- Error code
- Error message

This allows the frontend to associate validation errors with the corresponding form fields.

## Docker Deployment

For the complete application deployment, including the API, Angular frontend, and SQL Server, use the Docker Compose configuration in the repository root.

See the [main README](../README.md) for setup and execution instructions.
