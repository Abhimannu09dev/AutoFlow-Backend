using AutoFlow_Backend.Application.DTOs.Vehicles;
using FluentValidation;

namespace AutoFlow_Backend.Application.Validators.Vehicles;

public class VehicleCreateDtoValidator : AbstractValidator<VehicleCreateDto>
{
    public VehicleCreateDtoValidator()
    {
        RuleFor(x => x.VehicleNumber)
            .NotEmpty().WithMessage("Vehicle number is required.")
            .MaximumLength(20).WithMessage("Vehicle number cannot exceed 20 characters.");

        RuleFor(x => x.Brand)
            .NotEmpty().WithMessage("Brand is required.")
            .MaximumLength(50).WithMessage("Brand cannot exceed 50 characters.");

        RuleFor(x => x.Model)
            .NotEmpty().WithMessage("Model is required.")
            .MaximumLength(50).WithMessage("Model cannot exceed 50 characters.");

        RuleFor(x => x.Year)
            .InclusiveBetween(1900, DateTime.UtcNow.Year + 1)
            .WithMessage($"Year must be between 1900 and {DateTime.UtcNow.Year + 1}.");

        RuleFor(x => x.Mileage)
            .GreaterThanOrEqualTo(0).WithMessage("Mileage cannot be negative.");

        RuleFor(x => x.Color)
            .MaximumLength(30).WithMessage("Color cannot exceed 30 characters.")
            .When(x => x.Color != null);

        RuleFor(x => x.VIN)
            .MaximumLength(50).WithMessage("VIN cannot exceed 50 characters.")
            .When(x => x.VIN != null);
    }
}