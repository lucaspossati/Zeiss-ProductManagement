namespace Zeiss.ProductManagement.Application.Products.Queries;

public sealed record GetAllProductsQuery(int Page = 1, int PageSize = 10, string? SearchTerm = null, int? MinStock = null, int? MaxStock = null);

public sealed record GetProductByIdQuery(int ProductIdentifier);

public sealed record SearchProductsByNameQuery(string Name, int Page = 1, int PageSize = 10);

public sealed record GetProductsByStockLevelQuery(int? MinStock = null, int? MaxStock = null, int Page = 1, int PageSize = 10);
