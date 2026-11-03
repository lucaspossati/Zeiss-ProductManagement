namespace Zeiss.ProductManagement.Application.Products.Commands;

public sealed record CreateProductCommand(string Name, string? Description, decimal Price, int AvailableStock);

public sealed record UpdateProductCommand(int ProductIdentifier, string Name, string? Description, decimal Price, int AvailableStock);

public sealed record DeleteProductCommand(int ProductIdentifier);

public sealed record AddStockCommand(int ProductIdentifier, int Quantity);

public sealed record DecrementStockCommand(int ProductIdentifier, int Quantity);
