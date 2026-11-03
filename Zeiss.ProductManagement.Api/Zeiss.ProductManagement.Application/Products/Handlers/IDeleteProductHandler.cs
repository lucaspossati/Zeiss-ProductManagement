using Zeiss.ProductManagement.Application.Products.Commands;

namespace Zeiss.ProductManagement.Application.Products.Handlers;

public interface IDeleteProductHandler
{
    Task Handle(DeleteProductCommand command, CancellationToken cancellationToken);
}
