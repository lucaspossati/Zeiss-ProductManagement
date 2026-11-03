namespace Zeiss.ProductManagement.Domain.Products;

public class InvalidStockOperationException : ProductDomainException
{
    public InvalidStockOperationException(string message, string propertyName = "Quantity") : base(message, propertyName)
    {
    }
}
