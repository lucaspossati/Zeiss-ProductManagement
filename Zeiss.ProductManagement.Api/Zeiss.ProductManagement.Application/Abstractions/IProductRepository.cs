using Zeiss.ProductManagement.Domain.Products;

namespace Zeiss.ProductManagement.Application.Abstractions;

public interface IProductRepository
{
    Task<Product?> GetByIdentifierAsync(int productIdentifier, CancellationToken cancellationToken);
    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Product>> GetAllAsync(int page, int pageSize, string? searchTerm, int? minStock, int? maxStock, CancellationToken cancellationToken);
    Task<int> CountAsync(string? searchTerm, int? minStock, int? maxStock, CancellationToken cancellationToken);
    Task AddAsync(Product product, CancellationToken cancellationToken);
    Task UpdateAsync(Product product, CancellationToken cancellationToken);
    Task DeleteAsync(Product product, CancellationToken cancellationToken);
}
