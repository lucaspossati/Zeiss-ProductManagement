## Getting Started

**Live Demo:** [https://zeiss-product-management.me/products](https://zeiss-product-management.me/products)

The application is deployed and publicly accessible. The link above opens the Products page of the live application.

**Note:** The live demo uses a dedicated production environment with its own SQL Server database, isolated from the development environment.

## Docker Development

The recommended development setup uses Docker Compose to run the entire application.

From the repository root:

```powershell
docker compose up --build -d
```

This starts:

- ASP.NET Core API
- SQL Server database
- Angular frontend

The API runs inside the Docker network and is accessed by the frontend through the internal service address:

`http://zeiss-api:8080`

The API is not directly exposed to the host. All browser requests are routed through the frontend at:

http://localhost:4200

API requests use the `/api` path, which is forwarded to the backend by the Angular development proxy.

The API also supports live reload through `dotnet watch`.

### Useful Docker Commands

Check running services:

```powershell
docker compose ps
```

View API logs:

```powershell
docker compose logs -f zeiss-api
```

View database logs:

```powershell
docker compose logs -f zeiss-database
```

Stop the application:

```powershell
docker compose down
```

Database data is persisted in a Docker named volume and is preserved when containers are stopped or removed.

To remove the database volume as well:

```powershell
docker compose down -v
```

**Warning:** This permanently deletes the database data.

## API Configuration

The API is configured through environment variables in the root `.env` file.

The database connection uses the Docker service name:

`zeiss-database,1433`

This allows the API to communicate with SQL Server through the private Docker network without exposing the database port to the host.

The frontend communicates with the API using `/api`, rather than a hardcoded host address.

## Database Migrations

During development, the API applies pending migrations and initializes sample data on startup.

For production deployments, migrations are executed separately before starting the API:

```bash
docker compose run --rm --no-deps -e RUN_MIGRATIONS_ONLY=true zeiss-api
```

This avoids multiple API instances attempting to apply migrations concurrently.

## Local Development (Without Docker)

For development directly on the host, SQL Server must be running and the API connection string must be configured using .NET User Secrets.

Run the API from the backend directory:

```powershell
dotnet run --project .\Zeiss.ProductManagement.Api --launch-profile https
```

In this setup, the API listens on:

- HTTPS: `https://localhost:7068`
- HTTP: `http://localhost:5013`

This workflow is an alternative to the recommended Docker development setup.

## Build and Tests

Run from the backend directory:

```powershell
dotnet restore
dotnet build
dotnet test
```

## Docker Deployment

The backend includes a multi-stage Dockerfile targeting .NET 10.

The root `docker-compose.yml` orchestrates the API, frontend, and SQL Server.

For complete setup instructions, environment configuration, and deployment commands, see the [main README](../README.md).
