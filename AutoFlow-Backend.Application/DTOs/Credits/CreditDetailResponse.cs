namespace AutoFlow_Backend.Application.DTOs.Credits;

public class CreditDetailResponse
{
    public Guid SaleId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public DateTime SaleDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal TotalCreditAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public int DaysOverdue { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<CreditPaymentResponse> PaymentHistory { get; set; } = [];
}
