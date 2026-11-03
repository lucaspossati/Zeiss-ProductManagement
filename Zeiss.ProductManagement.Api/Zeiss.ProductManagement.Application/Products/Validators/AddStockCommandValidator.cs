using FluentValidation;
using Zeiss.ProductManagement.Application.Products.Commands;

namespace Zeiss.ProductManagement.Application.Products.Validators;

public sealed class AddStockCommandValidator : AbstractValidator<AddStockCommand>
{
    public AddStockCommandValidator()
    {
        RuleFor(command => command.ProductIdentifier)
            .InclusiveBetween(100000, 999999)
            .WithMessage("Product identifier must be a six-digit number.")
            .WithErrorCode("product.identifier.six_digits");

        RuleFor(command => command.Quantity)
            .GreaterThan(0)
            .WithMessage("Stock quantity must be greater than zero.")
            .WithErrorCode("product.stock.quantity_positive");
    }
}
