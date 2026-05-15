namespace AutoFlow_Backend.Application.DTOs.Credits;

public class CreditPaymentResponse
{
    public Guid PaymentId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? Note { get; set; }
}
