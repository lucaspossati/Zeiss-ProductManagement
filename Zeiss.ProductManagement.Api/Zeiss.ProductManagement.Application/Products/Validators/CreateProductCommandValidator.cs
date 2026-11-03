using FluentValidation;
using Zeiss.ProductManagement.Application.Products.Commands;

namespace Zeiss.ProductManagement.Application.Products.Validators;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(command => command.Name)
            .Cascade(CascadeMode.Stop)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Product name is required.")
            .WithErrorCode("product.name.required")
            .Must(name => name.Trim().Length is >= 2 and <= 200)
            .WithMessage("Product name must be between 2 and 200 characters long.")
            .WithErrorCode("product.name.length");

        RuleFor(command => command.Description)
            .MaximumLength(1000)
            .WithMessage("Product description cannot exceed 1000 characters.")
            .WithErrorCode("product.description.length");

        RuleFor(command => command.Price)
            .GreaterThan(0m)
            .WithMessage("Product price must be greater than zero.")
            .WithErrorCode("product.price.positive");

        RuleFor(command => command.AvailableStock)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Available stock cannot be negative.")
            .WithErrorCode("product.stock.non_negative");
    }
}
