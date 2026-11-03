using Zeiss.ProductManagement.Application.Products.Commands;

namespace Zeiss.ProductManagement.Application.Products.Handlers;

public interface IUpdateProductHandler
{
    Task<ProductDto> Handle(UpdateProductCommand command, CancellationToken cancellationToken);
}
