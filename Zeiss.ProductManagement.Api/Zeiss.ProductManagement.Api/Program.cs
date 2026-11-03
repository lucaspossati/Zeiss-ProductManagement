using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Zeiss.ProductManagement.Api.Contracts;
using Zeiss.ProductManagement.Api.Filters;
using Zeiss.ProductManagement.Api.Middleware;
using Zeiss.ProductManagement.Application.Products.Commands;
using Zeiss.ProductManagement.Infrastructure;
using Zeiss.ProductManagement.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddValidatorsFromAssemblyContaining<CreateProductCommand>();
builder.Services.AddScoped<FluentValidationActionFilter>();
builder.Services
    .AddControllers(options => options.Filters.AddService<FluentValidationActionFilter>())
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .SelectMany(entry => entry.Value?.Errors.Select(modelError =>
                    new ApiValidationError(
                        "Request",
                        string.IsNullOrWhiteSpace(entry.Key) || entry.Key == "$"
                            ? null
                            : entry.Key.Split('.').Last(),
                        "request.invalid",
                        string.IsNullOrWhiteSpace(modelError.ErrorMessage)
                            ? "The provided value is invalid."
                            : modelError.ErrorMessage)) ?? [])
                .ToArray();

            return new BadRequestObjectResult(
                BaseResponse<object>.Fail("One or more request values are invalid.", errors));
        };
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];
if (builder.Environment.IsDevelopment())
{
    allowedOrigins = allowedOrigins
        .Concat(["http://localhost:4200", "http://localhost:4201"])
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddProductManagement(builder.Configuration);

var app = builder.Build();

if (builder.Configuration.GetValue<bool>("RUN_MIGRATIONS_ONLY"))
{
    await InitializeDatabaseAsync(app.Services, app.Lifetime.ApplicationStopping);
    return;
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors("Frontend");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", async Task<IResult> (
    AppDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    if (!await dbContext.Database.CanConnectAsync(cancellationToken))
    {
        return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    }

    if (dbContext.Database.IsRelational()
        && (await dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
    {
        return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    }

    return Results.Ok(new { status = "Healthy" });
});

if (app.Environment.IsDevelopment())
{
    await InitializeDatabaseAsync(app.Services, app.Lifetime.ApplicationStopping);
}

app.Run();

static async Task InitializeDatabaseAsync(IServiceProvider services, CancellationToken cancellationToken)
{
    await using var scope = services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (dbContext.Database.IsRelational())
    {
        await dbContext.Database.MigrateAsync(cancellationToken);
    }
    else
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
    }

    await SeedData.InitializeAsync(dbContext, cancellationToken);
}
