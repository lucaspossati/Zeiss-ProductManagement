using Zeiss.ProductManagement.Application.Abstractions;
using Zeiss.ProductManagement.Application.Products.Queries;
using Zeiss.ProductManagement.Domain.Products;

namespace Zeiss.ProductManagement.Application.Products.Handlers;

public sealed class GetProductByIdHandler : IGetProductByIdHandler
{
    private readonly IProductRepository _repository;

    public GetProductByIdHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductDto> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdentifierAsync(query.ProductIdentifier, cancellationToken)
            ?? throw new ProductNotFoundException(query.ProductIdentifier);

        return ProductDto.FromDomain(product);
    }
}
