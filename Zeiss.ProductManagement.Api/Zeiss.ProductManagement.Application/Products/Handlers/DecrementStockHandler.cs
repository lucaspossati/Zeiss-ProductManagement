using Zeiss.ProductManagement.Application.Abstractions;
using Zeiss.ProductManagement.Application.Products.Commands;
using Zeiss.ProductManagement.Domain.Products;

namespace Zeiss.ProductManagement.Application.Products.Handlers;

public sealed class DecrementStockHandler : IDecrementStockHandler
{
    private readonly IProductRepository _repository;

    public DecrementStockHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductDto> Handle(DecrementStockCommand command, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdentifierAsync(command.ProductIdentifier, cancellationToken)
            ?? throw new ProductNotFoundException(command.ProductIdentifier);

        product.DecrementStock(command.Quantity);
        await _repository.UpdateAsync(product, cancellationToken);
        return ProductDto.FromDomain(product);
    }
}
