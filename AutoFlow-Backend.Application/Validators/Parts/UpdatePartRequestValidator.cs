using AutoFlow_Backend.Application.DTOs.Parts;
using FluentValidation;

namespace AutoFlow_Backend.Application.Validators.Parts;

public class UpdatePartRequestValidator : AbstractValidator<UpdatePartRequest>
{
    public UpdatePartRequestValidator()
    {
        RuleFor(x => x.PartName)
            .NotEmpty().WithMessage("Part name is required.")
            .MaximumLength(150).WithMessage("Part name cannot exceed 150 characters.");

        RuleFor(x => x.PartNumber)
            .NotEmpty().WithMessage("Part number is required.")
            .MaximumLength(100).WithMessage("Part number cannot exceed 100 characters.");

        RuleFor(x => x.Brand)
            .MaximumLength(100).WithMessage("Brand cannot exceed 100 characters.")
            .When(x => x.Brand != null);

        RuleFor(x => x.Category)
            .MaximumLength(100).WithMessage("Category cannot exceed 100 characters.")
            .When(x => x.Category != null);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
            .When(x => x.Description != null);

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Unit price must be greater than or equal to 0.");

        RuleFor(x => x.SellingPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Selling price must be greater than or equal to 0.");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity must be greater than or equal to 0.");

        RuleFor(x => x.MinimumStockLevel)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum stock level must be greater than or equal to 0.")
            .When(x => x.MinimumStockLevel.HasValue);

        RuleFor(x => x.VendorId)
            .NotEmpty().WithMessage("VendorId is invalid.")
            .When(x => x.VendorId.HasValue);
    }
}