using Zeiss.ProductManagement.Application.Abstractions;
using Zeiss.ProductManagement.Application.Products.Queries;

namespace Zeiss.ProductManagement.Application.Products.Handlers;

public sealed class SearchProductsByNameHandler : ISearchProductsByNameHandler
{
    private readonly IProductRepository _repository;

    public SearchProductsByNameHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaginatedResult<ProductDto>> Handle(SearchProductsByNameQuery query, CancellationToken cancellationToken)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 10 : query.PageSize;
        var searchTerm = string.IsNullOrWhiteSpace(query.Name) ? null : query.Name.Trim();

        var totalCount = await _repository.CountAsync(searchTerm, null, null, cancellationToken);
        var products = await _repository.GetAllAsync(page, pageSize, searchTerm, null, null, cancellationToken);

        return PaginatedResult<ProductDto>.Create(
            products.Select(ProductDto.FromDomain).ToList(),
            page,
            pageSize,
            totalCount);
    }
}
