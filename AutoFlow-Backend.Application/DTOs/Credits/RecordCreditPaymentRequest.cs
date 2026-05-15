using AutoFlow_Backend.Domain.Enums;

namespace AutoFlow_Backend.Application.DTOs.Credits;

public class RecordCreditPaymentRequest
{
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? Note { get; set; }
}
