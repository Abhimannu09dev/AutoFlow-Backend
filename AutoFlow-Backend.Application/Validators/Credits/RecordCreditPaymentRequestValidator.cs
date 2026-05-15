using AutoFlow_Backend.Application.DTOs.Credits;
using FluentValidation;

namespace AutoFlow_Backend.Application.Validators.Credits;

public class RecordCreditPaymentRequestValidator : AbstractValidator<RecordCreditPaymentRequest>
{
    public RecordCreditPaymentRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Payment amount must be greater than zero.");

        RuleFor(x => x.PaymentDate)
            .NotEmpty().WithMessage("Payment date is required.");

        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage("Invalid payment method.");

        RuleFor(x => x.Note)
            .MaximumLength(500).WithMessage("Note cannot exceed 500 characters.");
    }
}
