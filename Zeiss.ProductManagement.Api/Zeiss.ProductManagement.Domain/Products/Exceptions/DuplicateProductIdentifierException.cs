namespace Zeiss.ProductManagement.Domain.Products;

public class DuplicateProductIdentifierException : ProductDomainException
{
    public DuplicateProductIdentifierException(string message) : base(message)
    {
    }
}
