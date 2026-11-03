using Zeiss.ProductManagement.Application.Abstractions;
using Zeiss.ProductManagement.Application.Products.Commands;
using Zeiss.ProductManagement.Domain.Products;

namespace Zeiss.ProductManagement.Application.Products.Handlers;

public sealed class AddStockHandler : IAddStockHandler
{
    private readonly IProductRepository _repository;

    public AddStockHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductDto> Handle(AddStockCommand command, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdentifierAsync(command.ProductIdentifier, cancellationToken)
            ?? throw new ProductNotFoundException(command.ProductIdentifier);

        product.AddStock(command.Quantity);
        await _repository.UpdateAsync(product, cancellationToken);
        return ProductDto.FromDomain(product);
    }
}
