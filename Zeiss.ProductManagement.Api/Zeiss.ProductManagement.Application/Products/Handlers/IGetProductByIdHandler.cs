using Zeiss.ProductManagement.Application.Products.Queries;

namespace Zeiss.ProductManagement.Application.Products.Handlers;

public interface IGetProductByIdHandler
{
    Task<ProductDto> Handle(GetProductByIdQuery query, CancellationToken cancellationToken);
}
