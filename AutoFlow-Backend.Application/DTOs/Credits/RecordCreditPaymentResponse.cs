namespace AutoFlow_Backend.Application.DTOs.Credits;

public class RecordCreditPaymentResponse
{
    public Guid SaleId { get; set; }
    public decimal TotalCreditAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}
