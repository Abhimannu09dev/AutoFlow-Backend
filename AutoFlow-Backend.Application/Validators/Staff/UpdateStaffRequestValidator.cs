using AutoFlow_Backend.Application.DTOs.Staff;
using FluentValidation;

namespace AutoFlow_Backend.Application.Validators.Staff;

public class UpdateStaffRequestValidator : AbstractValidator<UpdateStaffRequest>
{
    public UpdateStaffRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(200).WithMessage("Full name cannot exceed 200 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(200).WithMessage("Email cannot exceed 200 characters.");

        RuleFor(x => x.Phone)
            .MaximumLength(30).WithMessage("Phone cannot exceed 30 characters.")
            .When(x => x.Phone != null);

        RuleFor(x => x.Address)
            .MaximumLength(300).WithMessage("Address cannot exceed 300 characters.")
            .When(x => x.Address != null);

        RuleFor(x => x.Position)
            .MaximumLength(100).WithMessage("Position cannot exceed 100 characters.")
            .When(x => x.Position != null);
    }
}