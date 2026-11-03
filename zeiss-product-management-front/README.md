# ZEISS Product Management — Frontend

Angular application for managing the product catalog and inventory.

## Requirements

- Node.js `22.22.3` or a version supported by Angular 22
- npm 10+
- Docker Desktop with Docker Compose
- The API and SQL Server running (see the backend README)

## Run the application locally

The recommended development setup runs SQL Server in Docker and runs the API
and frontend on the host:

1. Follow the [backend instructions](../Zeiss.ProductManagement.Api/README.md)
   to create `.env`, start SQL Server, and configure User Secrets.
2. Start the API with the `https` launch profile. It should be available at
   `https://localhost:7068`.
3. From this folder, install dependencies and start Angular:

```powershell
nvm use 22.22.3
npm ci
npm start
```

Open `http://localhost:4200`. The frontend requests
`https://localhost:7068/api/products`; the endpoint accepts `page` and
`pageSize`. If your browser does not trust the local API certificate, run this
once:

```powershell
dotnet dev-certs https --trust
```

The backend Docker Compose project starts SQL Server only. The API and frontend
are not included in that Compose project; in the workflow above, both run
locally on the host.

## Docker deployment

The production Dockerfile builds Angular and serves the static application with
Nginx. Nginx provides Angular Router fallback to `index.html` and proxies
`/api` to the backend over the private Docker network. The production API URL
is the relative path `/api`.

For the single Compose deployment, environment configuration, migrations,
health checks, and operational commands, follow the
[workspace deployment guide](../README.md).

## API configuration

The development API URL is defined in `src/environments/environment.ts`. If
you change it, keep the `/api` suffix and ensure the API allows the origin
`http://localhost:4200` through CORS.

Production builds use `src/environments/environment.production.ts`, whose
default URL is `/api`. Configure a reverse proxy to forward `/api` requests to
the backend in production.

## Project structure

```text
src/
  app/
    core/                 Application configuration, interceptors, shared services
    features/products/    Product pages, forms, models, and services
    shared/               Components and models reused across features
    app.config.ts         Standalone and zoneless application configuration
    app.routes.ts          Lazy-loaded routes
  environments/           Environment-specific API URLs
  styles/                 Global styles and design tokens
```

Product-specific components, contracts, and state stay within the `products`
feature. Shared folders are reserved for code with genuine reuse across
features.

## Build and tests

```powershell
npm run build
npm test
```

The production build is generated under `dist/`.
