## Getting Started

### Docker Development

The recommended development setup uses Docker Compose to run the entire application.

From the repository root:

```powershell
docker compose up --build -d
```

This starts:

- Angular development server
- ASP.NET Core API
- SQL Server database

Open the application at:

http://localhost:4200

The frontend communicates with the backend through `/api`. The Angular development proxy forwards these requests to the API container over the private Docker network.

No separate API URL or port configuration is required in the browser.

To view logs:

```powershell
docker compose logs -f
```

To stop the application:

```powershell
docker compose down
```

### Local Development (Without Docker)

If you prefer to run Angular directly on your machine, make sure SQL Server and the backend API are running first.

From the frontend directory:

```powershell
nvm use 22.22.3
npm ci
npm start
```

Open http://localhost:4200.

In this setup, the development environment uses the API URL configured in `src/environments/environment.ts`.

## API Configuration

The application uses environment-specific API configuration:

- `src/environments/environment.ts`: Local development.
- `src/environments/environment.production.ts`: Production.

In Docker development, `proxy.conf.json` forwards `/api` requests to `http://zeiss-api:8080`.

In production, Nginx serves the Angular application and proxies `/api` requests to the backend through the private Docker network.

The browser always uses the relative `/api` path in the production environment, avoiding hardcoded backend addresses.

## Docker Deployment

The frontend includes two Dockerfiles:

- `Dockerfile`: Builds the Angular application and serves it using Nginx.
- `Dockerfile.dev`: Runs the Angular development server with live reload.

The root `docker-compose.yml` orchestrates the frontend, backend, and SQL Server.

For complete setup instructions, see the [main README](../README.md).
