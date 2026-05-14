namespace AutoFlow_Backend.Application.DTOs.Sales;

public class SendInvoiceResponse
{
    public Guid SaleId { get; set; }
    public DateTime? InvoiceSentAt { get; set; }
    public DateTime? InvoiceFailedAt { get; set; }
    public string? InvoiceFailureReason { get; set; }
}
