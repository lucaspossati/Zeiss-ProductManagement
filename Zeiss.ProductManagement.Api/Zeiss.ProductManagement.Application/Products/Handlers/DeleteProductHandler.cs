using Zeiss.ProductManagement.Application.Abstractions;
using Zeiss.ProductManagement.Application.Products.Commands;
using Zeiss.ProductManagement.Domain.Products;

namespace Zeiss.ProductManagement.Application.Products.Handlers;

public sealed class DeleteProductHandler : IDeleteProductHandler
{
    private readonly IProductRepository _repository;

    public DeleteProductHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdentifierAsync(command.ProductIdentifier, cancellationToken)
            ?? throw new ProductNotFoundException(command.ProductIdentifier);

        await _repository.DeleteAsync(product, cancellationToken);
    }
}
