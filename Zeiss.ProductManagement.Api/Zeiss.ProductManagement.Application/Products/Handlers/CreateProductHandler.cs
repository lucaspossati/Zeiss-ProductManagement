using Zeiss.ProductManagement.Application.Abstractions;
using Zeiss.ProductManagement.Application.Products.Commands;
using Zeiss.ProductManagement.Domain.Products;

namespace Zeiss.ProductManagement.Application.Products.Handlers;

public sealed class CreateProductHandler : ICreateProductHandler
{
    private readonly IProductRepository _repository;
    private readonly IProductIdentifierGenerator _identifierGenerator;

    public CreateProductHandler(IProductRepository repository, IProductIdentifierGenerator identifierGenerator)
    {
        _repository = repository;
        _identifierGenerator = identifierGenerator;
    }

    public async Task<ProductDto> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var identifier = await _identifierGenerator.GenerateAsync(cancellationToken);
        var product = new Product(identifier, command.Name, command.Description, command.Price, command.AvailableStock);

        await _repository.AddAsync(product, cancellationToken);
        return ProductDto.FromDomain(product);
    }
}
