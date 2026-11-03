using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zeiss.ProductManagement.Infrastructure;
using Zeiss.ProductManagement.Worker.BackgroundServices;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddProductManagement(builder.Configuration);
builder.Services.AddHostedService<DatabaseInitializationWorker>();

await builder.Build().RunAsync();
