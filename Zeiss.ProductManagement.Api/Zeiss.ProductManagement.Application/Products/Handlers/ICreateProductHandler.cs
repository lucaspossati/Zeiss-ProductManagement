using Zeiss.ProductManagement.Application.Products.Commands;

namespace Zeiss.ProductManagement.Application.Products.Handlers;

public interface ICreateProductHandler
{
    Task<ProductDto> Handle(CreateProductCommand command, CancellationToken cancellationToken);
}
