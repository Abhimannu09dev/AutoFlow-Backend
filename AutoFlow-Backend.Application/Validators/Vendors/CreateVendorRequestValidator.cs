using AutoFlow_Backend.Application.DTOs.Vendors;
using FluentValidation;

namespace AutoFlow_Backend.Application.Validators.Vendors;

public class CreateVendorRequestValidator : AbstractValidator<CreateVendorRequest>
{
    public CreateVendorRequestValidator()
    {
        RuleFor(x => x.VendorName)
            .NotEmpty().WithMessage("Vendor name is required.")
            .MaximumLength(150).WithMessage("Vendor name cannot exceed 150 characters.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required.")
            .MaximumLength(30).WithMessage("Phone cannot exceed 30 characters.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email))
            .MaximumLength(200).WithMessage("Email cannot exceed 200 characters.");

        RuleFor(x => x.ContactPerson)
            .MaximumLength(150).WithMessage("Contact person cannot exceed 150 characters.");

        RuleFor(x => x.Address)
            .MaximumLength(300).WithMessage("Address cannot exceed 300 characters.");
    }
}