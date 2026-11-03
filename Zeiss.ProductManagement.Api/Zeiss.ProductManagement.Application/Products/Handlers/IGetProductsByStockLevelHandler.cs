using Zeiss.ProductManagement.Application.Products.Queries;

namespace Zeiss.ProductManagement.Application.Products.Handlers;

public interface IGetProductsByStockLevelHandler
{
    Task<PaginatedResult<ProductDto>> Handle(GetProductsByStockLevelQuery query, CancellationToken cancellationToken);
}
