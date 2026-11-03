using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zeiss.ProductManagement.Application.Abstractions;
using Zeiss.ProductManagement.Application.Products.Handlers;
using Zeiss.ProductManagement.Infrastructure.Persistence;
using Zeiss.ProductManagement.Infrastructure.Persistence.Repositories;

namespace Zeiss.ProductManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddProductManagement(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(connectionString)
            && string.Equals(
                Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
                "true",
                StringComparison.OrdinalIgnoreCase))
        {
            var sqlConnectionString = new SqlConnectionStringBuilder(connectionString);
            if (string.Equals(sqlConnectionString.DataSource, "localhost,1434", StringComparison.OrdinalIgnoreCase)
                || string.Equals(sqlConnectionString.DataSource, "localhost", StringComparison.OrdinalIgnoreCase))
            {
                sqlConnectionString.DataSource = "host.docker.internal,1434";
                connectionString = sqlConnectionString.ConnectionString;
            }
        }

        services.AddDbContext<AppDbContext>(options =>
        {
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                options.UseSqlServer(connectionString, sqlServerOptions =>
                {
                    sqlServerOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                });
            }
            else
            {
                options.UseInMemoryDatabase("ZeissProductManagement");
            }
        });

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductIdentifierGenerator, ProductIdentifierGenerator>();
        services.AddScoped<ICreateProductHandler, CreateProductHandler>();
        services.AddScoped<IUpdateProductHandler, UpdateProductHandler>();
        services.AddScoped<IDeleteProductHandler, DeleteProductHandler>();
        services.AddScoped<IAddStockHandler, AddStockHandler>();
        services.AddScoped<IDecrementStockHandler, DecrementStockHandler>();
        services.AddScoped<IGetAllProductsHandler, GetAllProductsHandler>();
        services.AddScoped<IGetProductByIdHandler, GetProductByIdHandler>();
        services.AddScoped<ISearchProductsByNameHandler, SearchProductsByNameHandler>();
        services.AddScoped<IGetProductsByStockLevelHandler, GetProductsByStockLevelHandler>();

        return services;
    }
}
