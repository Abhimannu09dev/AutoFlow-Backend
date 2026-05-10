using AutoFlow_Backend.Application.DTOs.Customers;
using FluentValidation;

namespace AutoFlow_Backend.Application.Validators.Customers;

public class CustomerPatchDtoValidator : AbstractValidator<CustomerPatchDto>
{
    public CustomerPatchDtoValidator()
    {
        RuleFor(x => x.FullName)
            .MaximumLength(150).WithMessage("Full name cannot exceed 150 characters.")
            .When(x => x.FullName != null);

        RuleFor(x => x.Phone)
            .MaximumLength(30).WithMessage("Phone cannot exceed 30 characters.")
            .When(x => x.Phone != null);

        RuleFor(x => x.Address)
            .MaximumLength(300).WithMessage("Address cannot exceed 300 characters.")
            .When(x => x.Address != null);
    }
}