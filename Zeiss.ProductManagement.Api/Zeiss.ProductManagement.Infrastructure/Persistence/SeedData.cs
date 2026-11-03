using Microsoft.EntityFrameworkCore;
using Zeiss.ProductManagement.Domain.Products;

namespace Zeiss.ProductManagement.Infrastructure.Persistence;

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext dbContext, CancellationToken cancellationToken = default)
    {
        var productsExist = await dbContext.Products.AnyAsync(cancellationToken);
        if (!productsExist)
        {
            var products = new[]
            {
                new Product(100000, "ZEISS Lens A", "Precision optical lens for industrial applications.", 149.99m, 14),
                new Product(100001, "ZEISS Sensor B", "High-precision sensor module for quality checks.", 299.50m, 8),
                new Product(100002, "ZEISS Calibration Kit", "Calibration kit for measurement devices.", 899.00m, 3)
            };

            dbContext.Products.AddRange(products);
        }

        var sequenceExists = await dbContext.ProductIdentifierSequences.AnyAsync(cancellationToken);
        if (!sequenceExists)
        {
            var currentValue = productsExist
                ? await dbContext.Products.MaxAsync(product => product.ProductIdentifier, cancellationToken)
                : 100002;
            var nextValue = currentValue >= 999999 ? 100000 : currentValue + 1;

            dbContext.ProductIdentifierSequences.Add(new ProductIdentifierSequence
            {
                CurrentValue = nextValue
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
