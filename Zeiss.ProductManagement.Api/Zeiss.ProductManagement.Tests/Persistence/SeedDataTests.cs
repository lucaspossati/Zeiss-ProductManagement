using Microsoft.EntityFrameworkCore;
using Zeiss.ProductManagement.Domain.Products;
using Zeiss.ProductManagement.Infrastructure.Persistence;

namespace Zeiss.ProductManagement.Tests.Persistence;

public sealed class SeedDataTests
{
    [Fact]
    public async Task InitializeAsync_creates_sequence_with_database_generated_identity()
    {
        await using var dbContext = CreateDbContext();

        await SeedData.InitializeAsync(dbContext);

        var sequence = await dbContext.ProductIdentifierSequences.SingleAsync();
        Assert.Equal(1, sequence.Id);
        Assert.Equal(100003, sequence.CurrentValue);
        Assert.Equal(3, await dbContext.Products.CountAsync());
    }

    [Fact]
    public async Task InitializeAsync_adds_sequence_after_products_already_exist()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Products.Add(new Product(123456, "Existing product", null, 10m, 1));
        await dbContext.SaveChangesAsync();

        await SeedData.InitializeAsync(dbContext);

        var sequence = await dbContext.ProductIdentifierSequences.SingleAsync();
        Assert.Equal(1, sequence.Id);
        Assert.Equal(123457, sequence.CurrentValue);
        Assert.Equal(1, await dbContext.Products.CountAsync());
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
