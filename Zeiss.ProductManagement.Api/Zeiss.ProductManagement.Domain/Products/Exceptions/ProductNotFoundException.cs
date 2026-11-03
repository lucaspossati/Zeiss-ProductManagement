namespace Zeiss.ProductManagement.Domain.Products;

public sealed class ProductNotFoundException : ProductDomainException
{
    public ProductNotFoundException(int productIdentifier)
        : base($"Product with identifier {productIdentifier} was not found.")
    {
    }
}
