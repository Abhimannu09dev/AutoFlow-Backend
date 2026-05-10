using AutoFlow_Backend.Application.DTOs.Customers;
using AutoFlow_Backend.Application.DTOs.Vehicles;
using AutoFlow_Backend.Application.Validators.Vehicles;
using FluentValidation;

namespace AutoFlow_Backend.Application.Validators.Customers;

public class CustomerCreateDtoValidator : AbstractValidator<CustomerCreateDto>
{
    public CustomerCreateDtoValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(150).WithMessage("Full name cannot exceed 150 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(200).WithMessage("Email cannot exceed 200 characters.");

        RuleFor(x => x.Phone)
            .MaximumLength(30).WithMessage("Phone cannot exceed 30 characters.");

        RuleFor(x => x.Address)
            .MaximumLength(300).WithMessage("Address cannot exceed 300 characters.");

        When(x => x.Vehicle != null, () =>
        {
            RuleFor(x => x.Vehicle!)
                .SetValidator(new VehicleCreateDtoValidator());
        });
    }
}