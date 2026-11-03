namespace Zeiss.ProductManagement.Api.Contracts;

public sealed record ApiValidationError(string Entity, string? Property, string Code, string Message);
