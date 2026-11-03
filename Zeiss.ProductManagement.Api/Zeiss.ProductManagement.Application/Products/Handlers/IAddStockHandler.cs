using Zeiss.ProductManagement.Application.Products.Commands;

namespace Zeiss.ProductManagement.Application.Products.Handlers;

public interface IAddStockHandler
{
    Task<ProductDto> Handle(AddStockCommand command, CancellationToken cancellationToken);
}
