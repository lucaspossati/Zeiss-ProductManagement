# ZEISS Product Management — Backend

.NET 10 backend for managing products and inventory. The solution separates
domain rules, application use cases, persistence, HTTP API, background
initialization, and tests into the `Domain`, `Application`, `Infrastructure`,
`Api`, `Worker`, and `Tests` projects.

## Requirements

- .NET 10 SDK
- Docker Desktop with Docker Compose
- Node.js `22.22.3` or a version supported by Angular 22 (for the frontend)
- `dotnet-ef` to create or manually apply migrations

For the complete Docker deployment, including the Angular frontend and SQL
Server, use the [workspace deployment guide](../README.md) and its root
`docker-compose.yml`.

Check the installed SDK and install the EF Core tool if necessary:

```powershell
dotnet --version
dotnet tool install --global dotnet-ef
```

## Start SQL Server with Docker

Run these commands from this folder, where `docker-compose.yml` is located.
Copy the environment variable template to `.env` and replace the example with
your own strong password:

```powershell
Copy-Item .env.example .env
notepad .env
docker compose up -d --wait sqlserver
docker compose ps
```

Docker Compose runs SQL Server 2022 Developer, publishes it at
`localhost,1434`, waits for its health check, and stores database files in the
Docker volume `sqlserver-data`. The local `.env` file is Git-ignored; do not
commit or share it.

To follow the startup logs:

```powershell
docker compose logs -f sqlserver
```

Stop the container without deleting the database:

```powershell
docker compose down
```

To also delete the database and all persisted data (irreversible):

```powershell
docker compose down --volumes
```

SQL Server sets the `sa` password when the volume is initialized. Changing
`.env` later does not change the password already stored in the database; keep
the original password or update the SQL Server login. Removing the volume
deletes all database data.

## Configure the API to use the database

For local development, run SQL Server in Docker and the API on the host.
Configure the API connection string in User Secrets, using the same password
as in `.env`. Run these commands from the backend root:

```powershell
$passwordLine = Get-Content .\.env | Where-Object { $_ -match '^MSSQL_SA_PASSWORD=' } | Select-Object -First 1
if (-not $passwordLine) { throw 'MSSQL_SA_PASSWORD was not found in .env' }
$env:MSSQL_SA_PASSWORD = $passwordLine -replace '^MSSQL_SA_PASSWORD=', ''

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1434;Database=ZeissProductManagement;User Id=sa;Password=$env:MSSQL_SA_PASSWORD;TrustServerCertificate=True;Encrypt=True;" --project .\Zeiss.ProductManagement.Api
Remove-Item Env:\MSSQL_SA_PASSWORD
```

User Secrets are stored outside the repository. Do not put passwords in
`appsettings.json` or other versioned files.

## Start the API and frontend

After SQL Server is healthy and the connection string is configured, start the
API from a terminal at the backend root:

```powershell
dotnet run --project .\Zeiss.ProductManagement.Api --launch-profile https
```

The `https` profile listens at `https://localhost:7068` and
`http://localhost:5013`. Trust the ASP.NET development certificate when
prompted. On startup, the API applies pending migrations and seeds sample data.

In another terminal, start Angular:

```powershell
Set-Location ..\zeiss-product-management-front
npm ci
npm start
```

Open `http://localhost:4200`. In development, the frontend uses
`https://localhost:7068/api`; the API allows the frontend origin
`http://localhost:4200` through CORS. To trust the local HTTPS certificate in
your browser, run this once:

```powershell
dotnet dev-certs https --trust
```

In this workflow, the API and frontend run on the host. This backend's Docker
Compose configuration starts SQL Server only; it does not start the API or
frontend.

## Migrations and Worker

In Development, the API applies migrations and initializes sample data at
startup. Production does not run migrations as part of API startup. Use the
one-shot migration command from the workspace root before starting the
production API:

```bash
docker compose run --rm --no-deps -e RUN_MIGRATIONS_ONLY=true zeiss-api
```

The Worker can also run as a separate process to apply migrations and
initialize sample data:

```powershell
dotnet run --project .\Zeiss.ProductManagement.Worker
```

Create a migration after changing the data model:

```powershell
dotnet ef migrations add MigrationName --project .\Zeiss.ProductManagement.Infrastructure --startup-project .\Zeiss.ProductManagement.Api --output-dir Persistence\Migrations
```

Apply migrations manually:

```powershell
dotnet ef database update --project .\Zeiss.ProductManagement.Infrastructure --startup-project .\Zeiss.ProductManagement.Api
```

The design-time factory uses the User Secrets configured for the API project.
When running the Worker separately, provide the same connection string through
its `ConnectionStrings__DefaultConnection` environment variable.

## Build and tests

Run these commands from the backend root:

```powershell
dotnet restore
dotnet build
dotnet test
```

## Main endpoints

| Method | Route | Description |
| --- | --- | --- |
| `GET` | `/api/products?page=1&pageSize=10` | Lists paginated products; accepts `search`, `minStock`, and `maxStock`. |
| `POST` | `/api/products` | Creates a product. |
| `GET` | `/api/products/{id}` | Gets a product by identifier. |
| `PUT` | `/api/products/{id}` | Updates a product. |
| `DELETE` | `/api/products/{id}` | Deletes a product. |
| `POST` | `/api/products/{id}/add-to-stock/{quantity}` | Adds stock. |
| `POST` | `/api/products/{id}/decrement-stock/{quantity}` | Decreases stock. |
| `GET` | `/api/products/search?name={name}` | Searches products by name. |
| `GET` | `/api/products/stock-level?min={min}&max={max}` | Filters products by stock level. |

FluentValidation, model binding, and domain rule errors are returned in
`BaseResponse.errors`, with an entity, an optional property, a code, and a
message. The frontend associates property errors with their form fields.
