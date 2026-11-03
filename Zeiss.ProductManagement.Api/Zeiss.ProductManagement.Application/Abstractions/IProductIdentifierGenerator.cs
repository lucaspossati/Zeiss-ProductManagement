namespace Zeiss.ProductManagement.Application.Abstractions;

public interface IProductIdentifierGenerator
{
    Task<int> GenerateAsync(CancellationToken cancellationToken);
}
