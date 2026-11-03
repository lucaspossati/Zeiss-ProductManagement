using Zeiss.ProductManagement.Application.Products.Queries;

namespace Zeiss.ProductManagement.Application.Products.Handlers;

public interface IGetAllProductsHandler
{
    Task<PaginatedResult<ProductDto>> Handle(GetAllProductsQuery query, CancellationToken cancellationToken);
}
