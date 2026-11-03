using Zeiss.ProductManagement.Application.Products.Queries;

namespace Zeiss.ProductManagement.Application.Products.Handlers;

public interface ISearchProductsByNameHandler
{
    Task<PaginatedResult<ProductDto>> Handle(SearchProductsByNameQuery query, CancellationToken cancellationToken);
}
