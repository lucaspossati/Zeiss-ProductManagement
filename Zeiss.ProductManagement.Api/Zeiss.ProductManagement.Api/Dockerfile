
# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
USER $APP_UID

# Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files first for better Docker layer caching
COPY ["Zeiss.ProductManagement.Api/Zeiss.ProductManagement.Api.csproj", "Zeiss.ProductManagement.Api/"]
COPY ["Zeiss.ProductManagement.Application/Zeiss.ProductManagement.Application.csproj", "Zeiss.ProductManagement.Application/"]
COPY ["Zeiss.ProductManagement.Domain/Zeiss.ProductManagement.Domain.csproj", "Zeiss.ProductManagement.Domain/"]
COPY ["Zeiss.ProductManagement.Infrastructure/Zeiss.ProductManagement.Infrastructure.csproj", "Zeiss.ProductManagement.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "Zeiss.ProductManagement.Api/Zeiss.ProductManagement.Api.csproj"

# Copy source code
COPY . .

# Build
WORKDIR /src/Zeiss.ProductManagement.Api
RUN dotnet build "Zeiss.ProductManagement.Api.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/build \
    --no-restore

# Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Zeiss.ProductManagement.Api.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false \
    --no-restore

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Zeiss.ProductManagement.Api.dll"]
