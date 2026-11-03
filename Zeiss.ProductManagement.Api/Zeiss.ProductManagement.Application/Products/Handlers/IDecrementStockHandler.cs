using Zeiss.ProductManagement.Application.Products.Commands;

namespace Zeiss.ProductManagement.Application.Products.Handlers;

public interface IDecrementStockHandler
{
    Task<ProductDto> Handle(DecrementStockCommand command, CancellationToken cancellationToken);
}
