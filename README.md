# ZEISS Product Management

A full-stack product catalog and inventory management application.

## Technologies

- **Backend:** ASP.NET Core 10
- **Frontend:** Angular 22
- **Database:** SQL Server 2022
- **Architecture:** Clean Architecture
- **Containerization:** Docker and Docker Compose

## Project Structure

```text
Zeiss-ProductManagement/
├── Zeiss.ProductManagement.Api/
│   ├── Zeiss.ProductManagement.Api/
│   ├── Zeiss.ProductManagement.Application/
│   ├── Zeiss.ProductManagement.Domain/
│   ├── Zeiss.ProductManagement.Infrastructure/
│   ├── Zeiss.ProductManagement.Worker/
│   └── Zeiss.ProductManagement.Tests/
│
├── zeiss-product-management-front/
│
├── docker-compose.yml
├── docker-compose.override.yml
├── .env.example
└── README.md
```

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) with Docker Compose
- Git

## Getting Started

**1. Clone the repository**

```bash
git clone <repository-url>
cd Zeiss-ProductManagement
```

**2. Configure environment variables**

Copy the example environment file:

```bash
cp .env.example .env
```

Open `.env` and configure your SQL Server password:

```env
MSSQL_SA_PASSWORD=Your_Strong_Password_Here
MSSQL_PID=Developer
FRONTEND_ORIGIN=http://localhost:4200
FRONTEND_BIND_ADDRESS=127.0.0.1
FRONTEND_PORT=8080
```

**3. Build and start the application**

```bash
docker compose up --build -d
```

Docker Compose starts the following services:

- SQL Server database
- ASP.NET Core API
- Angular frontend

Database migrations and initial data seeding are handled by the application setup.

## Access the Application

| Service          | URL                          |
| ---------------- | ---------------------------- |
| Frontend         | http://localhost:4200        |
| Backend API      | http://localhost:8080        |
| API Health Check | http://localhost:8080/health |

## Useful Docker Commands

Check running containers:

```bash
docker compose ps
```

View application logs:

```bash
docker compose logs -f
```

Stop the application:

```bash
docker compose down
```

Stop the application and remove database data:

```bash
docker compose down -v
```

**Warning:** The last command permanently deletes the SQL Server database volume.

## Running Tests

**Backend**

```bash
cd Zeiss.ProductManagement.Api
dotnet test
```

**Frontend**

```bash
cd zeiss-product-management-front
npm ci
npm test
npm run build
```

## Notes

- SQL Server data is persisted using a Docker named volume.
- The API and database communicate through a private Docker network.
- The frontend uses Nginx to serve the Angular application and proxy API requests.
- The `.env` file contains local configuration and must not be committed to the repository.
