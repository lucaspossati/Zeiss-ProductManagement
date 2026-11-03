using Zeiss.ProductManagement.Application.Abstractions;
using Zeiss.ProductManagement.Application.Products.Queries;

namespace Zeiss.ProductManagement.Application.Products.Handlers;

public sealed class GetProductsByStockLevelHandler : IGetProductsByStockLevelHandler
{
    private readonly IProductRepository _repository;

    public GetProductsByStockLevelHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaginatedResult<ProductDto>> Handle(GetProductsByStockLevelQuery query, CancellationToken cancellationToken)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 10 : query.PageSize;

        var totalCount = await _repository.CountAsync(null, query.MinStock, query.MaxStock, cancellationToken);
        var products = await _repository.GetAllAsync(page, pageSize, null, query.MinStock, query.MaxStock, cancellationToken);

        return PaginatedResult<ProductDto>.Create(
            products.Select(ProductDto.FromDomain).ToList(),
            page,
            pageSize,
            totalCount);
    }
}
