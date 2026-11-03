using Zeiss.ProductManagement.Application.Products.Commands;
using Zeiss.ProductManagement.Application.Products.Validators;

namespace Zeiss.ProductManagement.Tests.Products;

public sealed class ProductCommandValidatorTests
{
    [Fact]
    public async Task CreateProduct_reports_errors_for_invalid_properties()
    {
        var validator = new CreateProductCommandValidator();
        var command = new CreateProductCommand(" ", null, 0, -1);

        var result = await validator.ValidateAsync(command);

        Assert.Collection(
            result.Errors.OrderBy(error => error.PropertyName),
            error =>
            {
                Assert.Equal(nameof(CreateProductCommand.AvailableStock), error.PropertyName);
                Assert.Equal("product.stock.non_negative", error.ErrorCode);
            },
            error =>
            {
                Assert.Equal(nameof(CreateProductCommand.Name), error.PropertyName);
                Assert.Equal("product.name.required", error.ErrorCode);
            },
            error =>
            {
                Assert.Equal(nameof(CreateProductCommand.Price), error.PropertyName);
                Assert.Equal("product.price.positive", error.ErrorCode);
            });
    }

    [Fact]
    public async Task CreateProduct_accepts_values_matching_product_rules()
    {
        var validator = new CreateProductCommandValidator();
        var command = new CreateProductCommand("ZEISS Lens", "Camera lens", 99.95m, 12);

        var result = await validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task AddStock_requires_a_positive_quantity()
    {
        var validator = new AddStockCommandValidator();
        var command = new AddStockCommand(123456, 0);

        var result = await validator.ValidateAsync(command);

        var error = Assert.Single(result.Errors);
        Assert.Equal(nameof(AddStockCommand.Quantity), error.PropertyName);
        Assert.Equal("product.stock.quantity_positive", error.ErrorCode);
    }
}
