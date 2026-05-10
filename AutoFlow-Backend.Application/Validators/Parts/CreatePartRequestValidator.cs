using AutoFlow_Backend.Application.DTOs.Parts;
using FluentValidation;

namespace AutoFlow_Backend.Application.Validators.Parts;

public class CreatePartRequestValidator : AbstractValidator<CreatePartRequest>
{
    public CreatePartRequestValidator()
    {
        RuleFor(x => x.PartName)
            .NotEmpty().WithMessage("Part name is required.")
            .MaximumLength(150).WithMessage("Part name cannot exceed 150 characters.");

        RuleFor(x => x.PartNumber)
            .NotEmpty().WithMessage("Part number is required.")
            .MaximumLength(50).WithMessage("Part number cannot exceed 50 characters.");

        RuleFor(x => x.Brand)
            .MaximumLength(50).WithMessage("Brand cannot exceed 50 characters.");

        RuleFor(x => x.Category)
            .MaximumLength(50).WithMessage("Category cannot exceed 50 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Unit price must be greater than or equal to 0.");

        RuleFor(x => x.SellingPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Selling price must be greater than or equal to 0.");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity must be greater than or equal to 0.");

        RuleFor(x => x.MinimumStockLevel)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum stock level must be greater than or equal to 0.")
            .When(x => x.MinimumStockLevel.HasValue);
    }
}