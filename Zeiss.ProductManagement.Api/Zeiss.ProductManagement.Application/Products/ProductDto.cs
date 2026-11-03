namespace Zeiss.ProductManagement.Application.Products;

using Zeiss.ProductManagement.Domain.Products;

public sealed record ProductDto(
    int ProductIdentifier,
    string Name,
    string? Description,
    decimal Price,
    int AvailableStock,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc)
{
    public static ProductDto FromDomain(Product product) => new(
        product.ProductIdentifier,
        product.Name,
        product.Description,
        product.Price,
        product.AvailableStock,
        product.CreatedAtUtc,
        product.UpdatedAtUtc);
}
