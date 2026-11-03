namespace Zeiss.ProductManagement.Domain.Products;

public class InsufficientStockException : ProductDomainException
{
    public InsufficientStockException(string message, string propertyName = "AvailableStock") : base(message, propertyName)
    {
    }
}
