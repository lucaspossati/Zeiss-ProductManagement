namespace Zeiss.ProductManagement.Domain.Products;

public class ProductDomainException : Exception
{
    public ProductDomainException(string message, string? propertyName = null) : base(message)
    {
        PropertyName = propertyName;
    }

    public string? PropertyName { get; }
}
