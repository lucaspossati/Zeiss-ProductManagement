using Microsoft.EntityFrameworkCore;
using Zeiss.ProductManagement.Domain.Products;

namespace Zeiss.ProductManagement.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductIdentifierSequence> ProductIdentifierSequences => Set<ProductIdentifierSequence>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
