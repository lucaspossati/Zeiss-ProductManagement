using Zeiss.ProductManagement.Application.Abstractions;
using Zeiss.ProductManagement.Application.Products.Commands;
using Zeiss.ProductManagement.Domain.Products;

namespace Zeiss.ProductManagement.Application.Products.Handlers;

public sealed class UpdateProductHandler : IUpdateProductHandler
{
    private readonly IProductRepository _repository;

    public UpdateProductHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductDto> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdentifierAsync(command.ProductIdentifier, cancellationToken)
            ?? throw new ProductNotFoundException(command.ProductIdentifier);

        product.Update(command.Name, command.Description, command.Price, command.AvailableStock);
        await _repository.UpdateAsync(product, cancellationToken);
        return ProductDto.FromDomain(product);
    }
}
