using AutoFlow_Backend.Application.DTOs.Staff;
using FluentValidation;

namespace AutoFlow_Backend.Application.Validators.Staff;

public class CreateStaffRequestValidator : AbstractValidator<CreateStaffRequest>
{
    public CreateStaffRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(200).WithMessage("Full name cannot exceed 200 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(200).WithMessage("Email cannot exceed 200 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.");

        RuleFor(x => x.Phone)
            .MaximumLength(30).WithMessage("Phone cannot exceed 30 characters.");

        RuleFor(x => x.Address)
            .MaximumLength(300).WithMessage("Address cannot exceed 300 characters.");

        RuleFor(x => x.Role)
            .MaximumLength(50).WithMessage("Role cannot exceed 50 characters.");

        RuleFor(x => x.StaffCode)
            .MaximumLength(30).WithMessage("Staff code cannot exceed 30 characters.");
    }
}