using Microsoft.EntityFrameworkCore;
using Zeiss.ProductManagement.Application.Abstractions;
using Zeiss.ProductManagement.Domain.Products;

namespace Zeiss.ProductManagement.Infrastructure.Persistence.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly AppDbContext _dbContext;

    public ProductRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Product?> GetByIdentifierAsync(int productIdentifier, CancellationToken cancellationToken)
    {
        return await _dbContext.Products
            .FirstOrDefaultAsync(product => product.ProductIdentifier == productIdentifier, cancellationToken);
    }

    public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _dbContext.Products
            .FirstOrDefaultAsync(product => product.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(int page, int pageSize, string? searchTerm, int? minStock, int? maxStock, CancellationToken cancellationToken)
    {
        var query = _dbContext.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(product => product.Name.Contains(searchTerm));
        }

        if (minStock.HasValue)
        {
            query = query.Where(product => product.AvailableStock >= minStock.Value);
        }

        if (maxStock.HasValue)
        {
            query = query.Where(product => product.AvailableStock <= maxStock.Value);
        }

        return await query
            .OrderBy(product => product.ProductIdentifier)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(string? searchTerm, int? minStock, int? maxStock, CancellationToken cancellationToken)
    {
        var query = _dbContext.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(product => product.Name.Contains(searchTerm));
        }

        if (minStock.HasValue)
        {
            query = query.Where(product => product.AvailableStock >= minStock.Value);
        }

        if (maxStock.HasValue)
        {
            query = query.Where(product => product.AvailableStock <= maxStock.Value);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken)
    {
        await _dbContext.Products.AddAsync(product, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken)
    {
        _dbContext.Products.Update(product);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Product product, CancellationToken cancellationToken)
    {
        _dbContext.Products.Remove(product);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
